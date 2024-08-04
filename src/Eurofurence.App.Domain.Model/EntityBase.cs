using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Domain.Model
{
    [DataContract]
    public class EntityBase : IEntityBase
    {
        public EntityBase()
        {
            NewId();
            Touch();
        }

        [DataMember]
        [Required]
        public DateTime LastChangeDateTimeUtc { get; set; }

        [DataMember]
        [Required]
        public Guid Id { get; set; }

        [JsonIgnore]
        [Required]
        public int IsDeleted { get; set; }

        public void Touch()
        {
            LastChangeDateTimeUtc = DateTime.UtcNow;
        }

        public void NewId()
        {
            Id = Guid.NewGuid();
        }
    }
}