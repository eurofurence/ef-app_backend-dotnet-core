using Eurofurence.App.Domain.Model.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapRecord : EntityBase, IDtoRecordTransformable<MapRequest, MapResponse, MapRecord>
    {
        [DataMember]
        [Required]
        public string Description { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        [Required]
        public bool IsBrowseable { get; set; }

        [DataMember]
        public IList<MapEntryRecord> Entries { get; set; } = new List<MapEntryRecord>();

        [DataMember]
        public Guid? ImageId { get; set; }

        [JsonIgnore]
        public virtual ImageRecord Image { get; set; }

        // public void MergeDto(MapRequest source)
        // {
        //     var config = TypeAdapterConfig<MapRequest, MapRecord>
        //         .NewConfig()
        //         .Map(dest => dest.Description, src => src.Description)
        //         .Map(dest => dest.Order, src => src.Order)
        //         .Map(dest => dest.IsBrowseable, src => src.IsBrowseable)
        //         .Map(dest => dest.ImageId, src => src.ImageId)
        //         .Map(dest => dest.Entries, src =>
        //             src.Entries.Select(x => x.Transform()).ToList());
        //
        //     config.Compile();
        //     source.Adapt(this, config.Config);
        // }
        //
        // public MapResponse Transform()
        // {
        //     var config = TypeAdapterConfig<MapResponse, MapRecord>
        //         .NewConfig()
        //         .Map(dest => dest.Description, src => src.Description)
        //         .Map(dest => dest.Order, src => src.Order)
        //         .Map(dest => dest.IsBrowseable, src => src.IsBrowseable)
        //         .Map(dest => dest.ImageId, src => src.ImageId)
        //         .Map(dest => dest.Entries.Select(x =>
        //             x.Transform()).ToList(), src => src.Entries);
        //     config.Compile();
        //
        //     return this.Adapt<MapRecord, MapResponse>(config.Config);
        //
        // }
    }
}
