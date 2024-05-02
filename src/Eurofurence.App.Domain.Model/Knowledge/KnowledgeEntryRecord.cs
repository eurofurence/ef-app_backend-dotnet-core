using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeEntryRecord : EntityBase
    {
        [Required]
        [DataMember]
        public Guid KnowledgeGroupId { get; set; }

        [Required]
        [DataMember]
        public string Title { get; set; }

        [Required]
        [DataMember]
        public string Text { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember] 
        public List<LinkFragment> Links { get; set; } = new();

        [DataMember]
        public Guid[] ImageIds { get; set; }
    }
}