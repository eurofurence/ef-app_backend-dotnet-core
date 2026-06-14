using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events;

[DataContract]
public class EventConferenceRoomResponse : ResponseBase
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Description { get; set; }

    /// <summary>
    /// Deep link to the location of this event room in the map system (e.g. EFNav).
    /// </summary>
    [DataMember]
    public string MapLink { get; set; }

    /// <summary>
    /// <c>true</c> if room is exclusively used for internal events.
    /// </summary>
    [DataMember]
    public bool IsInternal { get; set; }

    /// <summary>
    /// Maximum number of people that can fit into the room.
    /// </summary>
    [DataMember]
    public int Capacity { get; set; }
}