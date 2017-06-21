using System;

namespace Eurofurence.App.Tools.CliToolBox.Importers.EventSchedule
{
    partial class CsvFileImporter
    {
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
