using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceRoomResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }

    /// <summary>
    /// Deep link to the location of this event room in the map system (e.g. EFNav)
    /// </summary>
    [DataMember]
    public string MapLink { get; set; }

    [DataMember]
    public bool IsInternal { get; set; }
}