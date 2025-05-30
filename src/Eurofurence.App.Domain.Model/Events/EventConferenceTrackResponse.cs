using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceTrackResponse
{
    [DataMember]
    public string ShortName { get; set; }

    [DataMember]
    public string LongName { get; set; }

    [DataMember]
    public string Color { get; set; }

    [DataMember]
    public string Icon { get; set; }

    [DataMember]
    public string IconColor { get; set; }

    [DataMember]
    public string Name { get; set; }
}