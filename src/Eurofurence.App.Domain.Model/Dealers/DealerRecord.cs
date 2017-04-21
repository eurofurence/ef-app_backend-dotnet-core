using Eurofurence.App.Domain.Model.Fragments;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Dealers
{
    [DataContract]
    public class DealerRecord : EntityBase
    {
        [Required]
        [DataMember]
        public int RegistrationNumber { get; set; }

        [Required]
        [DataMember]
        public string AttendeeNickname { get; set; }

        [Required]
        [DataMember]
        public string DisplayName { get; set; }

        [Required]
        [DataMember]
        public string ShortDescription { get; set; }

        [Required]
        [DataMember]
        public string AboutTheArtistText { get; set; }

        [Required]
        [DataMember]
        public string AboutTheArtText { get; set; }

        [Required]
        [DataMember]
        public LinkFragment[] Links { get; set; }

        [DataMember]
        public string ArtPreviewCaption { get; set; }

        [DataMember]
        public Guid? ArtistThumbnailImageId { get; set; }

        [DataMember]
        public Guid? ArtistImageId { get; set; }

        [DataMember]
        public Guid? ArtPreviewImageId { get; set; }
    }
}
