using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Domain.Model.Events
{
    /// <summary>
    /// Represents a event which was marked as a favorite by a user
    /// </summary>
    public class EventFavoriteRecord : EntityBase
    {
        /// <summary>
        /// The event that was marked as favorite
        /// </summary>
        [DataMember]
        public EventRecord Event { get; set; }

        /// <summary>
        /// The id of the event which was marked as a favorite
        /// </summary>
        [DataMember]
        public Guid EventId { get; set; }

        [JsonIgnore]
        public UserRecord User { get; set; }

        /// <summary>
        /// Identity provider ID of the user that submitted the registration.
        /// </summary>
        [DataMember]
        public string UserUid { get; set; }
    }
}