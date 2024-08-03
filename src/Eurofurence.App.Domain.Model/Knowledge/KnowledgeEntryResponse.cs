using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeEntryResponse : EntityBase
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
        public virtual List<LinkFragment> Links { get; set; } = new();

        [DataMember]
        public virtual List<Guid> ImageIds { get; set; } = new();
    }
}
