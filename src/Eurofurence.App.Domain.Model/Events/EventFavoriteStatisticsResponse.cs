using Eurofurence.App.Domain.Model.Users;
using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventFavoriteStatisticsResponse : ResponseBase
    {
        [DataMember]
        public Guid EventId { get; set; }

        [DataMember]
        public UserRegistrationStatus UserRegistrationStatus { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}
