using System.Collections.Generic;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Domain.Model.Users
{
    public class UserRecord
    {
        [DataMember]
        public string[] Roles { get; set; }

        [DataMember]
        public UserRegistration[] Registrations { get; set; }

    }
}