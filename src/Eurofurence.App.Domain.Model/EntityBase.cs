using System;
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
            IsDeleted = 0;
        }

        [DataMember]
        public DateTime LastChangeDateTimeUtc { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [JsonIgnore]
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