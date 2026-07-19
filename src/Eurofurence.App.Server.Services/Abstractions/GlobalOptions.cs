using System;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public class GlobalOptions
    {
        /// <summary>
        /// Unique identifier for the convention and iteration (e.g. "EF30").
        /// </summary>
        public string ConventionIdentifier { get; init; }
        /// <summary>
        /// Number of the conventions iteration (e.g. 30).
        /// </summary>
        public int ConventionNumber { get; init; }
        /// <summary>
        /// Full, official name of the convention.
        /// </summary>
        public string ConventionName { get; init; }
        /// <summary>
        /// Theme of the convention, usually used in conjunction with ConventionIdentifier or
        /// ConventionName and ConventionNumber.
        /// Example: "Eurofurence 30 – Fantastic Furry Festival"
        /// </summary>
        public string ConventionTheme { get; init; }
        /// <summary>
        /// Legal name of the organization hosting the convention.
        /// </summary>
        public string ConventionOrganization { get; init; }
        /// <summary>
        /// Official start of the convention (there may be events preceding this) in ISO 8601.
        /// </summary>
        public DateTimeOffset ConventionStartDateTime { get; init; }
        /// <summary>
        /// Official end of the convention (there may still be events after this) in ISO 8601.
        /// </summary>
        public DateTimeOffset ConventionEndDateTime { get; init; }
        /// <summary>
        /// Official name the convention venue can be found under e.g. on maps.
        /// </summary>
        public string ConventionVenueName { get; init; }
        /// <summary>
        /// Name of the city and/or region where the convention takes place (e.g. "Hamburg, Germany").
        /// </summary>
        public string ConventionVenueRegion { get; init; }
        /// <summary>
        /// Geographic latitude of the convention venue centroid.
        /// </summary>
        public double ConventionVenueLocationLatitude { get; init; }
        /// <summary>
        /// Geographic longitude of the convention venue centroid.
        /// </summary>
        public double ConventionVenueLocationLongitude { get; init; }
        /// <summary>
        /// URL of the convention's main website.
        /// </summary>
        public string ConventionWebsite { get; init; }
        /// <summary>
        /// URL or email address via which the convention can be contacted.
        /// </summary>
        public string ConventionContact { get; init; }
        /// <summary>
        /// TODO: Seems to be unused? Check and potentially drop during next cleanup.
        /// </summary>
        public string State { get; init; }
        /// <summary>
        /// URL under which the backend will be reachable.
        /// </summary>
        public string BaseUrl { get; init; }
        /// <summary>
        /// ID of the mobile app on the Apple App Store.
        /// </summary>
        public long AppIdITunes { get; init; }
        /// <summary>
        /// ID of the mobile app on the Google Play Store.
        /// </summary>
        public string AppIdPlay { get; init; }
        /// <summary>
        /// Base URL for all APIs.
        /// </summary>
        public string ApiBaseUrl => $"{BaseUrl}/Api";
        /// <summary>
        /// Base URL under which assets like CSS files for web previews can be found.
        /// </summary>
        public string ContentBaseUrl => $"{BaseUrl}";
        /// <summary>
        /// Base URL for web previews of entities.
        /// </summary>
        public string WebBaseUrl => $"{BaseUrl}/Web";
        /// <summary>
        /// Local directory used for volatile storage of temporary data (e.g. extracting import data).
        /// </summary>
        public string WorkingDirectory { get; init; }
    }
}