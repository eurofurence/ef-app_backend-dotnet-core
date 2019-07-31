using System;
using System.Linq;

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
            public string AppFeedback { get; set; }
            public string Tags { get; set; }
            public string CustomTags { get; set; }

            public string[] CalculateTags()
            {
                var tags = this.Tags.Split(",")
                    .Union(this.CustomTags.Split(","))
                    .Where(tag => !String.IsNullOrWhiteSpace(tag))
                    .Select(tag => tag.Trim())
                    .Select(tag => tag.Replace("fsps", "photoshoot"))
                    .ToArray();

                return tags;
            }
        }
    }
}
