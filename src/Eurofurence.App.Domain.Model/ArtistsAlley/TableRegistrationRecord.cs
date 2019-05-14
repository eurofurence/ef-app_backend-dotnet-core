using Eurofurence.App.Domain.Model.Fragments;
using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationRecord : EntityBase
    {
        public class StateChangeRecord
        {
            public DateTime ChangedDateTimeUtc{ get; set; }
            public string ChangedByUid { get; set; }
            public RegistrationStateEnum OldState { get; set; }
            public RegistrationStateEnum NewState { get; set; }
        }

        public enum RegistrationStateEnum
        {
            Pending = 0,
            Accepted = 1,
            Published = 2,
            Rejected = 3
        }

        public string OwnerUid { get; set; }

        public string DisplayName { get; set; }

        public string WebsiteUrl { get; set; }

        public string ShortDescription { get; set; }

        public ImageFragment Image { get; set; }

        public RegistrationStateEnum State { get; set; }

        public IList<StateChangeRecord> StateChangeLog { get; set; }

        public TableRegistrationRecord()
        {
            this.StateChangeLog = new List<StateChangeRecord>();
        }

        public void ChangeState(RegistrationStateEnum newState, string uid)
        {
            StateChangeLog.Add(new StateChangeRecord()
            {
                ChangedByUid = uid,
                ChangedDateTimeUtc = DateTime.UtcNow,
                NewState = newState,
                OldState = State
            });

            State = newState;
        }
    }
}
