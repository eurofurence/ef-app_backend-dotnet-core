using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
        public string Description { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool ShowInHamburgerMenu { get; set; }

        [DataMember]
        public string FontAwesomeIconName { get; set; }

        [JsonIgnore]
        public List<KnowledgeEntryRecord> KnowledgeEntries { get; set; }
    }
}