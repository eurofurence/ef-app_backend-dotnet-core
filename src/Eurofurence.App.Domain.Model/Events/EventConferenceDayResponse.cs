using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceDayResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public DateTime Date { get; set; }
}