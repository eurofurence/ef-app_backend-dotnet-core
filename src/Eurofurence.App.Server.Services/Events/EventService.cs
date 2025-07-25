using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Security.Claims;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Calendar = Ical.Net.Calendar;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventService : EntityServiceBase<EventRecord, EventResponse>,
        IEventService
    {
        private readonly ILogger _logger;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;
        private readonly EventOptions _eventOptions;
        private readonly AppDbContext _dbContext;

        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        private TimeSpan DateTimeOffset { get; set; }

        public EventService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService,
            ILoggerFactory loggerFactory,
            IOptions<EventOptions> eventOptions
        )
            : base(appDbContext, storageServiceFactory)
        {
            _dbContext = appDbContext;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;
            _eventOptions = eventOptions.Value;
            DateTimeOffset = TimeSpan.Zero;
            _logger = loggerFactory.CreateLogger(GetType());
        }


        public async Task AddEventToFavoritesIfNotExist([NotNull] ClaimsPrincipal user, EventRecord eventRecord)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            UserRecord userRecord = _dbContext.Users
                .Include(userRecord => userRecord.FavoriteEvents)
                .FirstOrDefault(x => x.IdentityId == user.GetSubject());

            if (userRecord != null && !userRecord.FavoriteEvents.Contains(eventRecord))
            {
                userRecord.FavoriteEvents.Add(eventRecord);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a list with all favorite events of a given user <paramref name="user"/>
        /// </summary>
        /// <param name="user">The user whose events should be fetched</param>
        /// <returns>A list of all the events of the user</returns>
        public List<EventRecord> GetFavoriteEventsFromUser(ClaimsPrincipal user)
        {
            return _dbContext.Users
                .AsNoTracking()
                .Include(x => x.FavoriteEvents)
                .Where(x => x.IdentityId == user.GetSubject())
                .Select(x => x.FavoriteEvents).FirstOrDefault();
        }

        public Calendar GetFavoriteEventsFromUserAsIcal(UserRecord user)
        {
            var favoriteEvents = user.FavoriteEvents;

            Calendar calendar = new Calendar();
            calendar.AddTimeZone(new VTimeZone("Europe/Berlin"));

            foreach (var item in favoriteEvents)
            {
                // Include for each event the title, start time/end time and the description of the event including
                // the organizer of the panel.
                CalendarEvent calendarEvent = new CalendarEvent()
                {
                    Summary = item.Title,
                    Description = item.Description + "\n" + $"Held by: {item.PanelHosts ?? "unknown fluff"}",
                    Start = new CalDateTime(item.StartDateTimeUtc),
                    End = new CalDateTime(item.EndDateTimeUtc),
                    Location = item.ConferenceRoom?.Name,
                };
                calendar.Events.Add(calendarEvent);
            }

            return calendar;
        }

        public async Task RemoveEventFromFavoritesIfExist(ClaimsPrincipal user, EventRecord eventRecord)
        {
            var foundRecord = _dbContext.Users
                .Include(x => x.FavoriteEvents)
                .First(x => x.IdentityId == user.GetSubject());

            if (foundRecord.FavoriteEvents.FirstOrDefault(x => x.Id == eventRecord.Id) is { } eventRecordToRemove)
            {
                foundRecord.FavoriteEvents.Remove(eventRecordToRemove);
            }

            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<EventRecord> FindConflicts(DateTime conflictStartTime, DateTime conflictEndTime,
            TimeSpan tolerance)
        {
            var queryConflictEndTime = conflictEndTime + tolerance;
            var queryConflictStartTime = conflictStartTime - tolerance;

            return FindAll().Where(e =>
                e.IsDeleted == 0 &&
                e.StartDateTimeUtc <= queryConflictEndTime &&
                e.EndDateTimeUtc >= queryConflictStartTime);
        }

        public async Task RunImportAsync()
        {
            try
            {
                await Semaphore.WaitAsync();
                _logger.LogDebug(LogEvents.Import, "Starting event import.");

                var httpClient = new HttpClient();
                var fileStream = await httpClient.GetStreamAsync(_eventOptions.Url);
                TextReader reader = new StreamReader(fileStream);

                using var csv = new CsvReader(reader,
                    new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," });
                csv.Context.RegisterClassMap<EventImportRowClassMap>();
                var csvRecords = csv.GetRecords<EventImportRow>().ToList();
                csvRecords = csvRecords
                    .Where(a => !a.Abstract.Equals("[CANCELLED]", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                if (csvRecords.Count == 0) return;

                foreach (var record in csvRecords)
                    record.ConferenceDayName = record.ConferenceDayName.Contains(" - ")
                        ? record.ConferenceDayName.Split(new[] { " - " }, StringSplitOptions.None)[1].Trim()
                        : record.ConferenceDayName.Trim();

                var conferenceTracks = csvRecords.Select(a => a.ConferenceTrack)
                    .Distinct().OrderBy(a => a).ToList();

                var conferenceRooms = csvRecords.Select(a => a.ConferenceRoom)
                    .Distinct().OrderBy(a => a).ToList();

                var conferenceDays = csvRecords.Select(a =>
                        new Tuple<DateTime, string>(
                            DateTime.SpecifyKind(DateTime.Parse(a.ConferenceDay, CultureInfo.InvariantCulture),
                                DateTimeKind.Utc),
                            a.ConferenceDayName))
                    .Distinct().OrderBy(a => a).ToList();

                int modifiedRecords = 0;

                var eventConferenceTracks = UpdateEventConferenceTracks(conferenceTracks, ref modifiedRecords);
                var eventConferenceRooms = UpdateEventConferenceRooms(conferenceRooms, ref modifiedRecords);
                var eventConferenceDays = UpdateEventConferenceDays(conferenceDays, ref modifiedRecords);
                UpdateEventEntries(csvRecords,
                    eventConferenceTracks,
                    eventConferenceRooms,
                    eventConferenceDays,
                    ref modifiedRecords);

                _logger.LogInformation(LogEvents.Import,
                    $"Event import finished successfully modifying {modifiedRecords} record(s).");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private List<EventConferenceDayRecord> UpdateEventConferenceDays(
            IList<Tuple<DateTime, string>> importConferenceDays,
            ref int modifiedRecords
        )
        {
            var eventConferenceDayRecords = _eventConferenceDayService.FindAll();

            var patch = new PatchDefinition<Tuple<DateTime, string>, EventConferenceDayRecord>(
                (source, list) => list.SingleOrDefault(a => a.Date == source.Item1)
            );

            patch.Map(s => s.Item1, t => t.Date)
                .Map(s => s.Item2, t => t.Name);

            var diff = patch.Patch(importConferenceDays, eventConferenceDayRecords);

            _eventConferenceDayService.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }


        private List<EventConferenceTrackRecord> UpdateEventConferenceTracks(
            IList<string> importConferenceTracks,
            ref int modifiedRecords
        )
        {
            var eventConferenceTrackRecords = _eventConferenceTrackService.FindAll();

            var patch = new PatchDefinition<string, EventConferenceTrackRecord>(
                (source, list) => list.SingleOrDefault(a => a.Name == source)
            );

            patch.Map(s => s, t => t.Name);
            var diff = patch.Patch(importConferenceTracks, eventConferenceTrackRecords);

            _eventConferenceTrackService.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        private List<EventConferenceRoomRecord> UpdateEventConferenceRooms(
            IList<string> importConferenceRooms,
            ref int modifiedRecords
        )
        {
            var eventConferenceRoomRecords = _eventConferenceRoomService.FindAll();

            var patch = new PatchDefinition<string, EventConferenceRoomRecord>(
                (source, list) => list.SingleOrDefault(a => a.Name == source)
            );

            var roomShortNameRegex = new Regex("(\\P{Pd}+)\\p{Pd}(\\P{Pd}+)");

            patch
                .Map(s => s, t => t.Name)
                .Map(s =>
                    {
                        if (roomShortNameRegex.IsMatch(s))
                        {
                            var matches = roomShortNameRegex.Matches(s);
                            return matches[0].Groups[1].Value.Trim();
                        }
                        else
                            return s;
                    },
                    t => t.ShortName);

            var diff = patch.Patch(importConferenceRooms, eventConferenceRoomRecords);

            _eventConferenceRoomService.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        private List<EventRecord> UpdateEventEntries(
            IList<EventImportRow> importEventEntries,
            IList<EventConferenceTrackRecord> currentConferenceTracks,
            IList<EventConferenceRoomRecord> currentConferenceRooms,
            IList<EventConferenceDayRecord> currentConferenceDays,
            ref int modifiedRecords
        )
        {
            var eventRecords = FindAll();

            var patch = new PatchDefinition<EventImportRow, EventRecord>(
                (source, list) => list.SingleOrDefault(a => a.SourceEventId == source.EventId)
            );

            patch.Map(s => s.EventId, t => t.SourceEventId)
                .Map(s => s.Slug, t => t.Slug)
                .Map(s => s.Title.Split('�')[0]?.Trim(), t => t.Title)
                .Map(s => (s.Title + '�').Split('�')[1]?.Trim(), t => t.SubTitle)
                .Map(s => s.Abstract, t => t.Abstract)
                .Map(
                    s => currentConferenceTracks.Single(a => a.Name == s.ConferenceTrack).Id,
                    t => t.ConferenceTrackId)
                .Map(
                    s => currentConferenceRooms.Single(a => a.Name == s.ConferenceRoom).Id,
                    t => t.ConferenceRoomId)
                .Map(
                    s => currentConferenceDays.Single(a => a.Name == s.ConferenceDayName).Id,
                    t => t.ConferenceDayId)
                .Map(s => s.Description, t => t.Description)
                .Map(s => s.Duration, t => t.Duration)
                .Map(s => s.StartTime, t => t.StartTime)
                .Map(s => s.EndTime, t => t.EndTime)
                .Map(s => DateTime.SpecifyKind(currentConferenceDays.Single(a => a.Name == s.ConferenceDayName)
                    .Date.Add(s.StartTime), DateTimeKind.Utc).AddHours(-2), t => t.StartDateTimeUtc)
                .Map(s => DateTime.SpecifyKind(currentConferenceDays.Single(a => a.Name == s.ConferenceDayName)
                        .Date.Add(s.EndTime).AddDays(s.StartTime < s.EndTime ? 0 : 1).AddHours(-2), DateTimeKind.Utc),
                    t => t.EndDateTimeUtc)
                .Map(s => s.PanelHosts, t => t.PanelHosts)
                .Map(s => s.AppFeedback.Equals("yes", StringComparison.InvariantCultureIgnoreCase),
                    t => t.IsAcceptingFeedback)
                .Map(s => s.CalculateTags(), t => t.Tags);

            var diff = patch.Patch(importEventEntries, eventRecords);

            ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public class EventImportRow
        {
            public int EventId { get; set; }
            public string Slug { get; set; }
            public string Title { get; set; }
            public string ConferenceTrack { get; set; }
            public string Abstract { get; set; }
            public string Description { get; set; }
            public string ConferenceDay { get; set; }
            public string ConferenceDayName { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public TimeSpan Duration { get; set; }
            public string ConferenceRoom { get; set; }
            public string PanelHosts { get; set; }
            public string AppFeedback { get; set; }
            public string Tags { get; set; }
            public string CustomTags { get; set; }

            public string[] CalculateTags()
            {
                var tags = this.Tags.Split(",")
                    .Union(this.CustomTags.Split(","))
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Select(tag => tag.Trim())
                    .Select(tag => tag.Replace("fsps", "photoshoot"))
                    .ToArray();

                return tags;
            }
        }

        public class EventImportRowClassMap : ClassMap<EventImportRow>
        {
            public EventImportRowClassMap()
            {
                Map(m => m.EventId).Name("event_id");
                Map(m => m.Slug).Name("slug");
                Map(m => m.Title).Name("title");
                Map(m => m.ConferenceTrack).Name("conference_track");
                Map(m => m.Abstract).Name("abstract");
                Map(m => m.Description).Name("description");
                Map(m => m.ConferenceDay).Name("conference_day");
                Map(m => m.ConferenceDayName).Name("conference_day_name");
                Map(m => m.StartTime).Name("start_time");
                Map(m => m.EndTime).Name("end_time");
                Map(m => m.Duration).Name("duration");
                Map(m => m.ConferenceRoom).Name("conference_room");
                Map(m => m.PanelHosts).Name("pannel_hosts");
                Map(m => m.AppFeedback).Name("appfeedback");
                Map(m => m.Tags).Name("tags");
                Map(m => m.CustomTags).Name("custom_tags");
            }
        }
    }
}