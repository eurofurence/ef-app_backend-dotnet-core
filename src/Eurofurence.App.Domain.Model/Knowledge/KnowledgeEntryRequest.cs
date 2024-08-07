using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeEntryRequest : EntityBase
    {
        [DataMember]
        public Guid KnowledgeGroupId { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public List<LinkFragment> Links { get; set; } = new();

        [DataMember]
        public List<Guid> ImageIds { get; set; } = new();
    }
}
