﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Knowledge
{
    [DataContract]
    public class KnowledgeEntryRecord : EntityBase, IDtoRecordTransformable<KnowledgeEntryRequest, KnowledgeEntryResponse, KnowledgeEntryRecord>
    {
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
        public DateTime? Published { get; set; }

        [Required]
        [DataMember]
        public Guid KnowledgeGroupId { get; set; }

        [JsonIgnore]
        public KnowledgeGroupRecord KnowledgeGroup { get; set; }

        [DataMember]
        public virtual List<LinkFragment> Links { get; set; } = new();

        [JsonIgnore]
        public virtual List<ImageRecord> Images { get; set; } = new();

    }
}
