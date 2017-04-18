using Eurofurence.App.Domain.Model.Fragments;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
        public LinkFragment[] Links { get; set; }
    }
}
