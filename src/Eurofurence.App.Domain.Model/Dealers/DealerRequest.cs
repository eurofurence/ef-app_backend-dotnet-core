#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Dealers;

[DataContract]
public class DealerRequest : IDtoTransformable<DealerRecord>
{
    /// <summary>
    /// Name under which this dealer is acting, e.G. name of the company or brand.
    /// </summary>
    [Required]
    [DataMember]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Brief description of merchandise/services offered.
    /// </summary>
    [Required]
    [DataMember]
    public required string Merchandise { get; set; }

    /// <summary>
    /// Short description/personal introduction about the dealer.
    /// </summary>
    [DataMember]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Variable length, bio of the artist/dealer.
    /// </summary>
    [DataMember]
    public string? AboutTheArtistText { get; set; }

    /// <summary>
    /// Variable length, description of the art/goods/services sold.
    /// </summary>
    [DataMember]
    public string? AboutTheArtText { get; set; }

    /// <summary>
    /// Link fragments to external website(s) of the dealer.
    /// </summary>
    [DataMember]
    public List<LinkFragment>? Links { get; set; } = new();

    /// <summary>
    /// Twitter handle of the dealer.
    /// </summary>
    [DataMember]
    public string? TwitterHandle { get; set; }

    /// <summary>
    /// Telegram handle of the dealer.
    /// </summary>
    [DataMember]
    public string? TelegramHandle { get; set; }

    /// <summary>
    /// Discord handle of the dealer.
    /// </summary>
    [DataMember]
    public string? DiscordHandle { get; set; }

    /// <summary>
    /// Mastodon handle of the dealer.
    /// </summary>
    [DataMember]
    public string? MastodonHandle { get; set; }

    /// <summary>
    /// Bluesky handle of the dealer.
    /// </summary>
    [DataMember]
    public string? BlueskyHandle { get; set; }

    /// <summary>
    /// Flag indicating whether the dealer is present at the dealers den on thursday.
    /// </summary>
    [DataMember]
    public bool? AttendsOnThursday { get; set; }

    /// <summary>
    /// Flag indicating whether the dealer is present at the dealers den on friday.
    /// </summary>
    [DataMember]
    public bool? AttendsOnFriday { get; set; }

    /// <summary>
    /// Flag indicating whether the dealer is present at the dealers den on saturday.
    /// </summary>
    [DataMember]
    public bool? AttendsOnSaturday { get; set; }

    /// <summary>
    /// Variable length, caption/subtext that describes the 'art preview' image.
    /// </summary>
    [DataMember]
    public string? ArtPreviewCaption { get; set; }

    /// <summary>
    /// ImageId of the thumbnail image (square) that represents the dealer.
    /// Used whenever multiple dealers are listed or a small, squared icon is needed.
    /// </summary>
    [DataMember]
    public Guid? ArtistThumbnailImageId { get; set; }

    /// <summary>
    /// ImageId of the artist image (any aspect ratio) that represents the dealer.
    /// Usually a personal photo / logo / badge, or a high-res version of the thumbnail image.
    /// </summary>
    [DataMember]
    public Guid? ArtistImageId { get; set; }

    /// <summary>
    /// ImageId of an art/merchandise sample sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public Guid? ArtPreviewImageId { get; set; }

    /// <summary>
    /// Flag indicating whether the dealer is located at the after dark dealers den.
    /// </summary>
    [DataMember]
    public bool? IsAfterDark { get; set; }

    /// <summary>
    /// List of standardized categories that apply to the goods/services sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public string[]? Categories { get; set; } = [];

    /// <summary>
    /// JSON string of keywords grouped by categories, that apply to the goods/services sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public Dictionary<string, string[]>? Keywords { get; set; }

}