using System;
using Mapster;

namespace Eurofurence.App.Domain.Model.Announcements;

[AdaptTo("[name]Dto"), GenerateMapper]
public class AnnounceMsg
{
    /// <summary>
    /// When does this announcement start to be valid?
    /// </summary>
    public DateTime ValidFromDateTimeUtc { get; set; }

    /// <summary>
    /// Until when will the announcement be valid?
    /// </summary>
    public DateTime ValidUntilDateTimeUtc { get; set; }

}