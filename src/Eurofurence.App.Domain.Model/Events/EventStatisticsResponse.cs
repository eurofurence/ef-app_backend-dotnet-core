#nullable enable
using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventStatisticsResponse : ResponseBase
{
    [DataMember]
    public string? Slug { get; set; }

    [DataMember]
    public string? Title { get; set; }

    [DataMember]
    public string? SubTitle { get; set; }

    [DataMember]
    public string? Abstract { get; set; }

    [DataMember]
    public Guid? ConferenceDayId { get; set; }

    [DataMember]
    public Guid? ConferenceTrackId { get; set; }

    [DataMember]
    public Guid? ConferenceRoomId { get; set; }

    [DataMember]
    public string? Description { get; set; }

    [DataMember]
    public TimeSpan? Duration { get; set; }

    [DataMember]
    public DateTime? StartDateTimeUtc { get; set; }

    [DataMember]
    public DateTime? EndDateTimeUtc { get; set; }

    [DataMember]
    public string? PanelHosts { get; set; }

    [DataMember]
    public bool? IsDeviatingFromConBook { get; set; }

    [DataMember]
    public bool? IsAcceptingFeedback { get; set; }

    /// <summary>
    ///     If set, refers to a banner ([3-4]:1 aspect ratio) that can be used when little
    ///     vertical space is available (e.G. event schedule, or a header section).
    /// </summary>
    [DataMember]
    public Guid? BannerImageId { get; set; }

    /// <summary>
    ///     If set, refers to an image of any aspect ratio that should be used where enough
    ///     vertical space is available (e.G. event detail).
    /// </summary>
    [DataMember]
    public Guid? PosterImageId { get; set; }

    [DataMember]
    public string[] Tags { get; set; } = [];

    [DataMember]
    public bool IsInternal { get; set; }

    /// <summary>
    ///     Total count of users, who have favored this event, regardless of whether they are checked in or not.
    /// </summary>
    [DataMember]
    public int FavoredByCount { get; set; }

    /// <summary>
    ///    Total count of users, who have favored this event and are currently checked in at the convention.
    /// </summary>
    [DataMember]
    public int FavoredByCheckedInCount { get; set; }

    /// <summary>
    ///    Total count of users, who have favored this event around the start time of the event.
    /// </summary>
    [DataMember]
    public int? FavoredByAtStartCount { get; set; }
}