using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using CsvHelper;
using CsvHelper.Configuration;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Server.Services.Abstractions;
using MongoDB.Driver;

namespace Eurofurence.App.Tools.EventScheduleImporter
{
    public class Program
    {

        public static List<EventConferenceDayRecord> UpdateEventConferenceDays(
            IList<Tuple<DateTime, string>> importConferenceDays,
            IEventConferenceDayService service
            )
        {
            var eventConferenceDayRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<Tuple<DateTime, string>, EventConferenceDayRecord>(
                (source, list) => list.SingleOrDefault(a => a.Date == source.Item1)
                );

            patch.Map(s => s.Item1, t => t.Date)
                .Map(s => s.Item2, t => t.Name);

            var diff = patch.Patch(importConferenceDays, eventConferenceDayRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }


        public static List<EventConferenceTrackRecord> UpdateEventConferenceTracks(
            IList<string> importConferenceTracks,
            IEventConferenceTrackService service
            )
        {
            var eventConferenceTrackRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<string, EventConferenceTrackRecord>(
                (source, list) => list.SingleOrDefault(a => a.Name == source)
                );

            patch.Map(s => s, t => t.Name);
            var diff = patch.Patch(importConferenceTracks, eventConferenceTrackRecords);

            service.ApplyPatchOperationAsync(diff).Wait();
            
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public static List<EventConferenceRoomRecord> UpdateEventConferenceRooms(
            IList<string> importConferenceRooms,
            IEventConferenceRoomService service
            )
        {
            var eventConferenceRoomRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<string, EventConferenceRoomRecord>(
                (source, list) => list.SingleOrDefault(a => a.Name == source)
                );

            patch.Map(s => s, t => t.Name);
            var diff = patch.Patch(importConferenceRooms, eventConferenceRoomRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public static List<EventRecord> UpdateEventEntries(
            IList<EventImportRow> ImportEventEntries,
            IList<EventConferenceTrackRecord> CurrentConferenceTracks,
            IList<EventConferenceRoomRecord> CurrentConferenceRooms,
            IList<EventConferenceDayRecord> CurrentConferenceDays,
            IEventService service
            )
        {
            var eventRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<EventImportRow, EventRecord>(
                (source, list) => list.SingleOrDefault(a => a.SourceEventId == source.EventId)
                );

            patch.Map(s => s.EventId, t => t.SourceEventId)
                .Map(s => s.Slug, t => t.Slug)
                .Map(s => s.Title.Split('|')[0], t => t.Title)
                .Map(s => (s.Title + '|').Split('|')[1], t => t.SubTitle)
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
                .Map(s => s.PanelHosts, t => t.PanelHosts);

            var diff = patch.Patch(ImportEventEntries, eventRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }


        public static void Main(string[] args)
        {
            var _client = new MongoClient("mongodb://127.0.0.1:27017");
            var _database = _client.GetDatabase("app_dev");

            Eurofurence.App.Domain.Model.MongoDb.BsonClassMapping.Register();
            
            var builder = new ContainerBuilder();
            builder.RegisterModule(new Domain.Model.MongoDb.DependencyResolution.AutofacModule(_database));
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule());

            var container = builder.Build();

            var eventConferenceTrackService = container.Resolve<IEventConferenceTrackService>();
            var eventConferenceRoomService = container.Resolve<IEventConferenceRoomService>();
            var eventConferenceDayService = container.Resolve<IEventConferenceDayService>();
            var eventService = container.Resolve<IEventService>();


            var stream = new FileStream(@"c:\temp\EF22-events-public-20160821T122601.541262456-v2.5.csv", FileMode.Open);
            TextReader r = new StreamReader(stream);

            var csv = new CsvReader(r);
            csv.Configuration.RegisterClassMap<EventImportRowClassMap>();
            var csvRecords = csv.GetRecords<EventImportRow>().ToList();

            var conferenceTracks = csvRecords.Select(a => a.ConferenceTrack)
                .Distinct().OrderBy(a => a).ToList();

            var conferenceRooms = csvRecords.Select(a => a.ConferenceRoom)
                .Distinct().OrderBy(a => a).ToList();

            var conferenceDays = csvRecords.Select(a => 
                new Tuple<DateTime, string>(DateTime.SpecifyKind(DateTime.Parse(a.ConferenceDay), DateTimeKind.Utc), a.ConferenceDayName))
                .Distinct().OrderBy(a => a).ToList();


            var eventConferenceTracks = UpdateEventConferenceTracks(conferenceTracks, eventConferenceTrackService);
            var eventConferenceRooms = UpdateEventConferenceRooms(conferenceRooms, eventConferenceRoomService);
            var eventConferenceDays = UpdateEventConferenceDays(conferenceDays, eventConferenceDayService);
            var eventEntries = UpdateEventEntries(csvRecords, 
                eventConferenceTracks,
                eventConferenceRooms,
                eventConferenceDays,
                eventService);
        }


        public sealed class EventImportRowClassMap : CsvClassMap<EventImportRow>
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
            }
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
        }
    }
}
