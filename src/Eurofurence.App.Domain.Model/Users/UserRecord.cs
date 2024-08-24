using System.Runtime.Serialization;

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