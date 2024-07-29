using Eurofurence.App.Domain.Model.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationResponse : EntityBase
    {
        [DataMember]
        public DateTime CreatedDateTimeUtc { get; set; }

        [DataMember]
        public string OwnerUid { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string WebsiteUrl { get; set; }

        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string TelegramHandle { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public ImageResponse Image { get; set; }

        [DataMember]
        public TableRegistrationRecord.RegistrationStateEnum State { get; set; }

        [IgnoreDataMember] public List<TableRegistrationRecord.StateChangeRecord> StateChangeLog { get; set; } = new();
    }
}
