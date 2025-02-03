
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventFavoriteRecord : EntityBase
    {

        /// <summary>
        /// The event that was formatted
        /// </summary>
        [DataMember]
        public EventRecord Event { get; set; }

        /// <summary>
        /// Identity provider ID of the user that submitted the registration.
        /// </summary>
        [DataMember]
        public string UserUid { get; set; }

        public EventFavoriteRecord(EventRecord eventRecord, string ownerUid)
        {
            this.Event = eventRecord;
            this.UserUid = ownerUid;
        }

    }
}