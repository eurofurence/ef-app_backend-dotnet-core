using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceRoomResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string ShortName { get; set; }
}