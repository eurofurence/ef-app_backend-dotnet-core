#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Dealers;

public class DealerResponse
{

    /// <summary>
    /// **(pba)** Name under which this dealer is acting, e.G. name of the company or brand. 
    /// </summary>
    [Required]
    [DataMember]
    public required string DisplayName { get; set; }

    /// <summary>
    /// **(pba)** Brief description of merchandise/services offered.
    /// </summary>
    [Required]
    [DataMember]
    public required string Merchandise { get; set; }

    /// <summary>
    /// **(pba)** Short description/personal introduction about the dealer.
    /// </summary>
    [DataMember]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// **(pba)** Variable length, bio of the artist/dealer.
    /// </summary>
    [DataMember]
    public string? AboutTheArtistText { get; set; }

    /// <summary>
    /// **(pba)** Variable length, description of the art/goods/services sold.
    /// </summary>
    [DataMember]
    public string? AboutTheArtText { get; set; }

    /// <summary>
    /// **(pba)** Link fragments to external website(s) of the dealer.
    /// </summary>
    [DataMember]
    public List<LinkFragment>? Links { get; set; } = new();

    /// <summary>
    /// **(pba)** Twitter handle of the dealer.
    /// </summary>
    [DataMember]
    public string? TwitterHandle { get; set; }

    /// <summary>
    /// **(pba)** Telegram handle of the dealer.
    /// </summary>
    [DataMember]
    public string? TelegramHandle { get; set; }

    /// <summary>
    /// **(pba)** Discord handle of the dealer.
    /// </summary>
    [DataMember]
    public string? DiscordHandle { get; set; }

    /// <summary>
    /// **(pba)** Mastodon handle of the dealer.
    /// </summary>
    [DataMember]
    public string? MastodonHandle { get; set; }

    /// <summary>
    /// **(pba)** Bluesky handle of the dealer.
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
    /// **(pba)** Variable length, caption/subtext that describes the 'art preview' image.
    /// </summary>
    [DataMember]
    public string? ArtPreviewCaption { get; set; }

    /// <summary>
    /// **(pba)** ImageId of the thumbnail image (square) that represents the dealer.
    /// Used whenever multiple dealers are listed or a small, squared icon is needed.
    /// </summary>
    [DataMember]
    public Guid? ArtistThumbnailImageId { get; set; }

    /// <summary>
    /// **(pba)** ImageId of the artist image (any aspect ratio) that represents the dealer.
    /// Usually a personal photo / logo / badge, or a high-res version of the thumbnail image.
    /// </summary>
    [DataMember]
    public Guid? ArtistImageId { get; set; }

    /// <summary>
    /// **(pba)** ImageId of an art/merchandise sample sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public Guid? ArtPreviewImageId { get; set; }

    /// <summary>
    /// Flag indicating whether the dealer is located at the after dark dealers den.
    /// </summary>
    [DataMember]
    public bool? IsAfterDark { get; set; }

    /// <summary>
    /// **(pba)** List of standardized categories that apply to the goods/services sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public string[]? Categories { get; set; } = [];

    /// <summary>
    /// **(pba)** JSON string of keywords grouped by categories, that apply to the goods/services sold/offered by the dealer.
    /// </summary>
    [DataMember]
    public Dictionary<string, string[]>? Keywords { get; set; }

}