using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventConferenceTrackRecord : EntityBase, IDtoTransformable<EventConferenceTrackResponse>
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

        [JsonIgnore]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}