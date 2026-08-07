using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Domain.Model.Users;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventFavoriteStatisticsRecord : EntityBase, IDtoTransformable<EventFavoriteStatisticsResponse>
    {
        [DataMember]
        public Guid EventId { get; set; }

        [JsonIgnore]
        public EventRecord Event { get; set; }

        [DataMember]
        public UserRegistrationStatus UserRegistrationStatus { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}
