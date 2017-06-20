using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapEntryRecord
    {
        [DataMember]
        [Required]
        public Guid Id { get; set; }

        [DataMember]
        public double RelativeX { get; set; }

        [DataMember]
        public double RelativeY { get; set; }

        [DataMember]
        public double RelativeTapRadius { get; set; }

        [DataMember]
        public LinkFragment Link { get; set; }

        [IgnoreDataMember]
        public virtual MapRecord Map { get; set; }
    }
}