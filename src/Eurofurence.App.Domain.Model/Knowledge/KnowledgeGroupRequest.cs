using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeGroupRequest : IDtoTransformable<KnowledgeGroupRecord>
    {
        [Required]
        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public string Description { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool ShowInHamburgerMenu { get; set; }

        [DataMember]
        public string FontAwesomeIconName { get; set; }
    }
}