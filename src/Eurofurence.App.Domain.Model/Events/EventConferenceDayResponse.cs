using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceDayResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public DateTime Date { get; set; }

    /// <summary>
    /// <c>true</c> if there are exclusively internal events on this day.
    /// </summary>
    [DataMember]
    public bool IsInternal { get; set; }
}