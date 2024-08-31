using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationRecord : EntityBase
    {
        public class StateChangeRecord : EntityBase
        {
            public DateTime ChangedDateTimeUtc { get; set; }
            public string ChangedByUid { get; set; }
            public RegistrationStateEnum OldState { get; set; }
            public RegistrationStateEnum NewState { get; set; }
        }

        /// <summary>
        /// Potential states a registration can be in.
        /// </summary>
        public enum RegistrationStateEnum
        {
            /// <summary>
            /// Registration has been submitted and is pending review.
            /// </summary>
            Pending = 0,
            /// <summary>
            /// Registration has been reviewed and accepted, and may be published.
            /// </summary>
            Accepted = 1,
            /// <summary>
            /// Registration has been published to social media channels.
            /// </summary>
            Published = 2,
            /// <summary>
            /// Registration has been rejected and must be submitted again.
            /// </summary>
            Rejected = 3
        }

        /// <summary>
        /// Date and time at which the registration was submitted.
        /// </summary>
        [DataMember]
        public DateTime CreatedDateTimeUtc { get; set; }

        /// <summary>
        /// Identity provider ID of the user that submitted the registration.
        /// </summary>
        [DataMember]
        public string OwnerUid { get; set; }

        /// <summary>
        /// Actual name of the user (may be different from `DisplayName`).
        /// </summary>
        [DataMember]
        public string OwnerUsername { get; set; }

        /// <summary>
        /// Preferred display name of artist.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional URL of artist's website.
        /// </summary>
        [DataMember]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Short text provided by the artist on who they are and what they are offering.
        /// </summary>
        [DataMember]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Optional Telegram handle (prefixed @ will be removed automatically).
        /// </summary>
        [DataMember]
        public string TelegramHandle { get; set; }

        /// <summary>
        /// Table number at which the artist has seated themselves in the Artist Alley.
        /// Must be > 0 and may have optional, upper limit. 
        /// </summary>
        [DataMember]
        public string Location { get; set; }

        /// <summary>
        /// ID of the image attached to this registration.
        /// </summary>
        [DataMember]
        public Guid? ImageId { get; set; }

        /// <summary>
        /// Metadata of the image attached to this registration.
        /// </summary>
        [DataMember]
        public ImageRecord Image { get; set; }

        /// <summary>
        /// Current state of the registration (Pending, Accepted, Published, Rejected).
        /// </summary>
        [DataMember]
        public RegistrationStateEnum State { get; set; } = RegistrationStateEnum.Pending;

        /// <summary>
        /// Internal log of state changes this registration has undergone.
        /// </summary>
        [JsonIgnore]
        public IList<StateChangeRecord> StateChangeLog { get; set; }

        public TableRegistrationRecord()
        {
            this.StateChangeLog = new List<StateChangeRecord>();
        }

        public StateChangeRecord ChangeState(RegistrationStateEnum newState, string uid)
        {
            var stateChange = new StateChangeRecord()
            {
                ChangedByUid = uid,
                ChangedDateTimeUtc = DateTime.UtcNow,
                NewState = newState,
                OldState = State
            };

            StateChangeLog.Add(stateChange);

            State = newState;

            return stateChange;
        }
    }
}
