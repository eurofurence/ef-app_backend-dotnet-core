using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventConferenceRoomRecord : EntityBase, IDtoTransformable<EventConferenceRoomResponse>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ShortName { get; set; }

        [DataMember]
        public bool IsInternal { get; set; }

        [JsonIgnore]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}