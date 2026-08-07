using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventConferenceTrackRecord : EntityBase, IDtoTransformable<EventConferenceTrackResponse>
    {
        [JsonIgnore]
        public int SourceId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// RGB color by which the track can be identified, encoded as hexadecimal string in <c>#rrggbb</c>
        /// format.
        /// </summary>
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public bool IsInternal { get; set; }

        [JsonIgnore]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}