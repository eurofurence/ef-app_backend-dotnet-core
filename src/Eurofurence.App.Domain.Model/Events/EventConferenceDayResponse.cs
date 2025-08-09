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

    [DataMember]
    public bool IsInternal { get; set; }
}