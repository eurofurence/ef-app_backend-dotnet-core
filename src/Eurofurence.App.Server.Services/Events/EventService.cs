using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using System.Net.Http.Json;
using Eurofurence.App.Domain.Model.Events.Pretalx;
using System.Collections.Immutable;
using AngleSharp.Common;

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
        private readonly AppDbContext _appDbContext;

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
            _appDbContext = appDbContext;
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

            UserRecord userRecord = _appDbContext.Users
                .Include(userRecord => userRecord.FavoriteEvents)
                .FirstOrDefault(x => x.IdentityId == user.GetSubject());

            if (userRecord != null && !userRecord.FavoriteEvents.Contains(eventRecord))
            {
                userRecord.FavoriteEvents.Add(eventRecord);
            }

            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a list with all favorite events of a given user <paramref name="user"/>
        /// </summary>
        /// <param name="user">The user whose events should be fetched</param>
        /// <returns>A list of all the events of the user</returns>
        public List<EventRecord> GetFavoriteEventsFromUser(ClaimsPrincipal user)
        {
            return _appDbContext.Users
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
                // TODO: Check user authorisation for internal events if we wish to allow them in
                //       favorites iCal? An issue would only arise if somebody favs internal events
                //       and then is removed from staff, which should likely not be an issue.
                if (item.IsInternal) continue;

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
            var foundRecord = _appDbContext.Users
                .Include(x => x.FavoriteEvents)
                .First(x => x.IdentityId == user.GetSubject());

            if (foundRecord.FavoriteEvents.FirstOrDefault(x => x.Id == eventRecord.Id) is { } eventRecordToRemove)
            {
                foundRecord.FavoriteEvents.Remove(eventRecordToRemove);
            }

            await _appDbContext.SaveChangesAsync();
        }

        public IQueryable<EventRecord> FindConflicts(DateTime conflictStartTime, DateTime conflictEndTime,
            TimeSpan tolerance, bool includeInternal)
        {
            var queryConflictEndTime = conflictEndTime + tolerance;
            var queryConflictStartTime = conflictStartTime - tolerance;

            return FindAll(e => includeInternal || !e.IsInternal).Where(e =>
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
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _eventOptions.ApiKey);
                var pretalxSchedule = await httpClient.GetFromJsonAsync<PretalxSchedule>($"{_eventOptions.ApiUrl}/events/{_eventOptions.EventSlug}/schedules/latest/?expand=slots,slots.room,slots.submission,slots.submission.speakers,slots.submission.submission_type,slots.submission.track");

                var tags = new List<PretalxTag>();
                var tagsUrl = $"{_eventOptions.ApiUrl}/events/{_eventOptions.EventSlug}/tags/";
                do
                {
                    var tagsPage = await httpClient.GetFromJsonAsync<PretalxPage<PretalxTag>>(tagsUrl);
                    tags.AddRange(tagsPage.Results);
                    tagsUrl = tagsPage.Next;
                } while (tagsUrl != null);

                // For some reason, slots in a published Pretalx schedule can come without a
                // start/end time, so we have to filter them out.
                var slots = pretalxSchedule.Slots.Where(slot => slot.Start != null && slot.End != null);
                var slotsPublic = slots.Where(slot => slot.Submission.Tags.Contains(_eventOptions.InternalTagId));

                var tracks = slots.Select(slot => slot.Submission.Track).ToHashSet();
                var tracksPublic = slotsPublic.Select(slot => slot.Submission.Track).ToHashSet();

                var rooms = slots.Select(slot => slot.Room).ToHashSet();
                var roomsPublic = slotsPublic.Select(slot => slot.Room).ToHashSet();

                var days = _eventOptions.EventDays.Select(eventDay => Tuple.Create(DateTime.Parse($"{eventDay.Key}T00:00:00Z"), eventDay.Value)).ToList();
                var daysPublic = slotsPublic.Select(slot => slot.Start.Value.Date).ToHashSet();

                var eventConferenceTracks = await UpdateEventConferenceTracksAsync(tracks, tracksPublic);
                var eventConferenceRooms = await UpdateEventConferenceRoomsAsync(rooms, roomsPublic);
                var eventConferenceDays = await UpdateEventConferenceDaysAsync(days, daysPublic);
                var eventEntries = await UpdateEventEntriesAsync(slots,
                    eventConferenceTracks.Item2,
                    eventConferenceRooms.Item2,
                    eventConferenceDays.Item2,
                    tags.ToDictionary(tag => tag.Id, tag => tag));

                _logger.LogInformation(LogEvents.Import,
                    $"Event import finished successfully modifying {eventConferenceTracks.Item1} EventConferenceTrack(s), {eventConferenceRooms.Item1} EventConferenceRoom(s), {eventConferenceDays.Item1} EventConferenceDay(s) and {eventEntries.Item1} Event(s).");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task<Tuple<int, List<EventConferenceDayRecord>>> UpdateEventConferenceDaysAsync(
            IList<Tuple<DateTime, string>> importDays,
            ISet<DateTime> importDaysPublic
        )
        {
            var eventConferenceDayRecords = _appDbContext.EventConferenceDays.AsNoTracking();

            var patch = new PatchDefinition<Tuple<DateTime, string>, EventConferenceDayRecord>(
                (source, list) => list.SingleOrDefault(a => a.Date == source.Item1)
            );

            patch.Map(source => source.Item1, target => target.Date)
                .Map(source => source.Item2, target => target.Name)
                .Map(source => !importDaysPublic.Contains(source.Item1), target => target.IsInternal);

            var diff = patch.Patch(importDays, eventConferenceDayRecords);

            await _eventConferenceDayService.ApplyPatchOperationAsync(diff);

            var modifiedRecords = diff.Count(a => a.Action != ActionEnum.NotModified);
            return new Tuple<int, List<EventConferenceDayRecord>>(modifiedRecords, diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList());
        }


        private async Task<Tuple<int, List<EventConferenceTrackRecord>>> UpdateEventConferenceTracksAsync(
            ISet<PretalxTrack> importTracks,
            ISet<PretalxTrack> importTracksPublic
        )
        {
            var eventConferenceTrackRecords = _appDbContext.EventConferenceTracks.AsNoTracking();
            var patch = new PatchDefinition<PretalxTrack, EventConferenceTrackRecord>(
                (source, list) => list.SingleOrDefault(target => target.SourceId == source.Id)
            );

            patch.Map(source => source.Id, target => target.SourceId)
                .Map(source => source.Name.GetValueOrDefault(_eventOptions.DefaultLocale), target => target.Name)
                .Map(source => source.Description.GetValueOrDefault(_eventOptions.DefaultLocale), target => target.Description)
                .Map(source => source.Color, target => target.Color)
                .Map(source => !importTracksPublic.Contains(source), target => target.IsInternal);
            var diff = patch.Patch(importTracks, eventConferenceTrackRecords);

            await _eventConferenceTrackService.ApplyPatchOperationAsync(diff);

            var modifiedRecords = diff.Count(a => a.Action != ActionEnum.NotModified);
            return new Tuple<int, List<EventConferenceTrackRecord>>(modifiedRecords, diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList());
        }

        private async Task<Tuple<int, List<EventConferenceRoomRecord>>> UpdateEventConferenceRoomsAsync(
            ISet<PretalxRoom> importRooms,
            ISet<PretalxRoom> importRoomsPublic
        )
        {
            var eventConferenceRoomRecords = _appDbContext.EventConferenceRooms.AsNoTracking();

            var patch = new PatchDefinition<PretalxRoom, EventConferenceRoomRecord>(
                (source, targets) => targets.SingleOrDefault(target => target.SourceId == source.Id)
            );

            patch.Map(source => source.Id, target => target.SourceId)
                .Map(source => source.Name.GetValueOrDefault(_eventOptions.DefaultLocale), target => target.Name)
                .Map(source => source.Description.GetValueOrDefault(_eventOptions.DefaultLocale), target => target.Description)
                .Map(source => source.Name.GetValueOrDefault(_eventOptions.DefaultLocale).Split('–')[0]?.Trim(), target => target.ShortName)
                .Map(source => source.Capacity, target => target.Capacity)
                .Map(source => !importRoomsPublic.Contains(source), target => target.IsInternal);

            var diff = patch.Patch(importRooms, eventConferenceRoomRecords);

            await _eventConferenceRoomService.ApplyPatchOperationAsync(diff);

            var modifiedRecords = diff.Count(a => a.Action != ActionEnum.NotModified);
            return new Tuple<int, List<EventConferenceRoomRecord>>(modifiedRecords, diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList());
        }

        private async Task<Tuple<int, List<EventRecord>>> UpdateEventEntriesAsync(
            IEnumerable<PretalxSlot> importEventEntries,
            IList<EventConferenceTrackRecord> currentConferenceTracks,
            IList<EventConferenceRoomRecord> currentConferenceRooms,
            IList<EventConferenceDayRecord> currentConferenceDays,
            IDictionary<int, PretalxTag> tags
        )
        {
            var eventRecords = FindAll();

            var patch = new PatchDefinition<PretalxSlot, EventRecord>(
                (source, targets) => targets.SingleOrDefault(target => target.SourceId == source.Id)
            );

            patch.Map(source => source.Id, target => target.SourceId)
                .Map(source => $"{source.Submission.Code}-{source.Id}", target => target.Slug)
                .Map(source => source.Submission.Title.Split('–')[0]?.Trim(), target => target.Title)
                .Map(source => (source.Submission.Title + '–').Split('–')[1]?.Trim(), target => target.SubTitle)
                .Map(source => source.Submission.Abstract, target => target.Abstract)
                .Map(source => source.Submission.Description ?? source.Description, target => target.Description)
                .Map(
                    source => currentConferenceTracks.Single(track => track.SourceId == source.Submission.Track.Id).Id,
                    target => target.ConferenceTrackId)
                .Map(
                    source => currentConferenceRooms.SingleOrDefault(room => room.SourceId == source.Room?.Id).Id,
                    target => target.ConferenceRoomId)
                .Map(
                    source => currentConferenceDays.SingleOrDefault(day => day.Date == source.Start?.Date).Id,
                    target => target.ConferenceDayId)
                .Map(source => TimeSpan.FromMinutes(source.Duration), target => target.Duration)
                .Map(source => source.Start.Value, target => target.StartDateTimeUtc)
                .Map(source => source.End.Value, target => target.EndDateTimeUtc)
                .Map(source => string.Join(", ", source.Submission.Speakers.Select(speaker => speaker.Name)), target => target.PanelHosts)
                .Map(source => source.Submission.Tags.Contains(_eventOptions.AcceptsFeedbackTagId),
                    target => target.IsAcceptingFeedback)
                .Map(source => source.Submission.Tags.Select(tagId => tags.GetOrDefault(tagId, null)?.Tag).ToArray(), target => target.Tags)
                .Map(source => source.Submission.Tags.Contains(_eventOptions.InternalTagId), target => target.IsInternal);

            var diff = patch.Patch(importEventEntries, eventRecords);

            await ApplyPatchOperationAsync(diff);

            var modifiedRecords = diff.Count(a => a.Action != ActionEnum.NotModified);
            return new Tuple<int, List<EventRecord>>(modifiedRecords, diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList());
        }
    }
}