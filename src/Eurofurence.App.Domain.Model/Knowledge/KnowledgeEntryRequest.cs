using System;
using System.Collections.Generic;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    public class KnowledgeEntryRequest
    {
        public Guid KnowledgeGroupId { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public int Order { get; set; }

        public List<LinkFragment> Links { get; set; } = new();

        public List<Guid> ImageIds { get; set; } = new();
    }
}
