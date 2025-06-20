﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventConferenceDayRecord : EntityBase, IDtoTransformable<EventConferenceDayResponse>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [JsonIgnore]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}