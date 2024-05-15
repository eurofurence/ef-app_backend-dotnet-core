using System;

namespace Eurofurence.App.Domain.Model.Images
{
    public class ImageContentRecord : EntityBase
    {
        public Guid ImageId { get; set; }
        public ImageRecord Image { get; set; }
        public byte[] Content { get; set; }
    }
}