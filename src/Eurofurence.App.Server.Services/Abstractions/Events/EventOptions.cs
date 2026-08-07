using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public class EventOptions
    {
        /// <summary>
        /// Base-URL to the Pretalx API, usually ending in <c>/api</c>
        /// </summary>
        public string ApiUrl { get; init; }
        /// <summary>
        /// Read-only API Key generated in Pretalx. Should have read list & details for events,
        /// submissions, speakers, rooms, tags, tracks, schedules, slots, submission-types
        /// </summary>
        public string ApiKey { get; init; }
        /// <summary>
        /// Slug identifying the actual event within the Pretalx instance.
        /// </summary>
        public string EventSlug { get; init; }
        /// <summary>
        /// i18n is currently not used, thus a default locale for multi-language API fields must be
        /// provided, specifying which language string should be used.
        /// </summary>
        public string DefaultLocale { get; init; }
        /// <summary>
        /// ID of the Pretalx tag that identifies internal events (visible to staff only).
        /// </summary>
        public int InternalTagId { get; init; }
        /// <summary>
        /// ID of the Pretalx track containing internal events (visible to staff only).
        /// </summary>
        public int InternalTrackId { get; init; }
        /// <summary>
        /// Make all events internal (visible to staff only) independent of their track or tags.
        /// Can be used during pre-production phase when schedule is not yet public, but in staff-
        /// internal preview.
        /// </summary>
        public bool MarkAllInternal { get; init; }
        /// <summary>
        /// ID of the Pretalx tag marking events which accept feedback to be provided via the app.
        /// </summary>
        public int AcceptsFeedbackTagId { get; init; }
        /// <summary>
        /// Maps ISO 8601 dates of convention days to display names for those days.
        /// Example:
        ///     1970-01-01 => Mon – Staff Arrival
        /// </summary>
        public Dictionary<string, string> EventDays { get; init; }
    }
}
