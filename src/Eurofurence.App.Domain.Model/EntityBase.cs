using System;
using System.ComponentModel.DataAnnotations;
using Eurofurence.App.Common.Abstractions;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model
{
    [DataContract]
    public class EntityBase : IEntityBase
    {
        [DataMember]
        [Required]
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        public DateTime LastChangeDateTimeUtc { get; set; }

        [IgnoreDataMember]
        [Required]
        public int IsDeleted { get; set; }

        public EntityBase()
        {
            NewId();
            Touch();
        }

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