using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Users
{
    public class UserRegistration
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public UserRegistrationStatus Status { get; set; }
    }
}