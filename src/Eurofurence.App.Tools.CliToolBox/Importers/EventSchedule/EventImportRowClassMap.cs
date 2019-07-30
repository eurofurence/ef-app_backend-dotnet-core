using CsvHelper.Configuration;

namespace Eurofurence.App.Tools.CliToolBox.Importers.EventSchedule
{
    partial class CsvFileImporter
    {
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
