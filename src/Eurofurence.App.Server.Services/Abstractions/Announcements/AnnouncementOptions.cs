#nullable enable
namespace Eurofurence.App.Server.Services.Abstractions.Announcements
{
    public class AnnouncementOptions
    {
        /// <summary>
        /// URL of the endpoint providing the latest announcements.
        /// </summary>
        public string? Url { get; init; }
        /// <summary>
        /// Hours from creation that an announcement should be considered valid.
        /// Defaults to half a year.
        /// </summary>
        public int AnnouncementValidityHours { get; init; } = 24 * 183;
    }
}