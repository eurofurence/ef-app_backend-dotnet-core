using Eurofurence.App.Domain.Model.Users;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventFavoriteStatisticsResponse : ResponseBase
    {
        [DataMember]
        public Guid EventId { get; set; }

        [DataMember]
        public UserRegistrationStatus UserRegistrationStatus { get; set; } = new UserRegistrationStatus();

        [DataMember]
        public int Count { get; set; }
    }
}
