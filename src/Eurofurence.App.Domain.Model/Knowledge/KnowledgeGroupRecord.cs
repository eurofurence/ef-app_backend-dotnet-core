using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeGroupRecord : EntityBase
    {
        [Required]
        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool ShowInHamburgerMenu { get; set; }

        [DataMember]
        public char FontAwesomeIconCharacter { get; set; }
    }
}
