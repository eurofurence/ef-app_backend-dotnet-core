using System;
using System.ComponentModel.DataAnnotations;
using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Domain.Model
{
    public class EntityBase : IEntityBase
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public DateTime LastChangeDateTimeUtc { get; set; }

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