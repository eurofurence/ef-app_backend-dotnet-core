using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

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
        public string Merchandise { get; set; }

        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string AboutTheArtistText { get; set; }

        [DataMember]
        public string AboutTheArtText { get; set; }

        [Required]
        [DataMember]
        public LinkFragment[] Links { get; set; }

        [DataMember]
        public string TwitterHandle { get; set; }

        [DataMember]
        public string TelegramHandle { get; set; }

        [DataMember]
        public bool AttendsOnThursday { get; set; }

        [DataMember]
        public bool AttendsOnFriday { get; set; }

        [DataMember]
        public bool AttendsOnSaturday { get; set; }

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