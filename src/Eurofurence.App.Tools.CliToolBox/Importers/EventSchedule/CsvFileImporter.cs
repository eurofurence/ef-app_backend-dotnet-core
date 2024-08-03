using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Tools.CliToolBox.Importers.EventSchedule
{
    partial class CsvFileImporter
    {
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;

        public DateTime? FakeStartDate { get; set; }
        private TimeSpan DateTimeOffset { get; set; }

        public CsvFileImporter(
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService
        )
        {
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;

            DateTimeOffset = TimeSpan.Zero;
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
                .Map(s => {
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
            IList<EventImportRow> ImportEventEntries,
            IList<EventConferenceTrackRecord> CurrentConferenceTracks,
            IList<EventConferenceRoomRecord> CurrentConferenceRooms,
            IList<EventConferenceDayRecord> CurrentConferenceDays,
            ref int modifiedRecords
        )
        {
            var eventRecords = _eventService.FindAll();

            var patch = new PatchDefinition<EventImportRow, EventRecord>(
                (source, list) => list.SingleOrDefault(a => a.SourceEventId == source.EventId)
            );

            patch.Map(s => s.EventId, t => t.SourceEventId)
                .Map(s => s.Slug, t => t.Slug)
                .Map(s => s.Title.Split('–')[0]?.Trim(), t => t.Title)
                .Map(s => (s.Title + '–').Split('–')[1]?.Trim(), t => t.SubTitle)
                .Map(s => s.Abstract, t => t.Abstract)
                .Map(
                    s => CurrentConferenceTracks.Single(a => a.Name == s.ConferenceTrack).Id,
                    t => t.ConferenceTrackId)
                .Map(
                    s => CurrentConferenceRooms.Single(a => a.Name == s.ConferenceRoom).Id,
                    t => t.ConferenceRoomId)
                .Map(
                    s => CurrentConferenceDays.Single(a => a.Name == s.ConferenceDayName).Id,
                    t => t.ConferenceDayId)
                .Map(s => s.Description, t => t.Description)
                .Map(s => s.Duration, t => t.Duration)
                .Map(s => s.StartTime, t => t.StartTime)
                .Map(s => s.EndTime, t => t.EndTime)
                .Map(s => DateTime.SpecifyKind(CurrentConferenceDays.Single(a => a.Name == s.ConferenceDayName)
                    .Date.Add(s.StartTime), DateTimeKind.Utc).AddHours(-2), t => t.StartDateTimeUtc)
                .Map(s => DateTime.SpecifyKind(CurrentConferenceDays.Single(a => a.Name == s.ConferenceDayName)
                        .Date.Add(s.EndTime).AddDays(s.StartTime < s.EndTime ? 0 : 1).AddHours(-2), DateTimeKind.Utc),
                    t => t.EndDateTimeUtc)
                .Map(s => s.PanelHosts, t => t.PanelHosts)
                .Map(s => s.AppFeedback.Equals("yes", StringComparison.InvariantCultureIgnoreCase), t => t.IsAcceptingFeedback)
                .Map(s => s.CalculateTags(), t => t.Tags);

            var diff = patch.Patch(ImportEventEntries, eventRecords);

            _eventService.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public int ImportCsvFile(string inputPath)
        {
            var stream = new FileStream(inputPath, FileMode.Open);
            TextReader reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," });
            csv.Context.RegisterClassMap<EventImportRowClassMap>();
            var csvRecords = csv.GetRecords<EventImportRow>().ToList();
            csvRecords = csvRecords
                .Where(a => !a.Abstract.Equals("[CANCELLED]", StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (csvRecords.Count == 0) return 0;

            foreach (var record in csvRecords)
                record.ConferenceDayName = record.ConferenceDayName.Contains(" - ")
                    ? record.ConferenceDayName.Split(new[] { " - " }, StringSplitOptions.None)[1].Trim()
                    : record.ConferenceDayName.Trim();

            var conferenceTracks = csvRecords.Select(a => a.ConferenceTrack)
                .Distinct().OrderBy(a => a).ToList();

            var conferenceRooms = csvRecords.Select(a => a.ConferenceRoom)
                .Distinct().OrderBy(a => a).ToList();

            var conferenceDays = csvRecords.Select(a =>
                    new Tuple<DateTime, string>(DateTime.SpecifyKind(DateTime.Parse(a.ConferenceDay, new CultureInfo("de-DE")), DateTimeKind.Utc),
                        a.ConferenceDayName))
                .Distinct().OrderBy(a => a).ToList();

            int modifiedRecords = 0;

            if (FakeStartDate.HasValue)
            {
                DateTimeOffset = TimeSpan.FromDays((FakeStartDate.Value - conferenceDays.Min(a => a.Item1)).Days);
                for (int i = 0; i < conferenceDays.Count; i++)
                {
                    conferenceDays[i] = new Tuple<DateTime, string>(
                        conferenceDays[i].Item1.Add(DateTimeOffset),
                        conferenceDays[i].Item2
                    );
                }
            }

            var eventConferenceTracks = UpdateEventConferenceTracks(conferenceTracks, ref modifiedRecords);
            var eventConferenceRooms = UpdateEventConferenceRooms(conferenceRooms, ref modifiedRecords);
            var eventConferenceDays = UpdateEventConferenceDays(conferenceDays, ref modifiedRecords);
            var eventEntries = UpdateEventEntries(csvRecords,
                eventConferenceTracks,
                eventConferenceRooms,
                eventConferenceDays,
                ref modifiedRecords);

            return modifiedRecords;
        }
    }
}
