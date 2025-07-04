using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceTrackResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }
}