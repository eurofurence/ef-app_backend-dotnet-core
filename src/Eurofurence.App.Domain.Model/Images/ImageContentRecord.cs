using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Domain.Model.Images
{
    public class ImageContentRecord : EntityBase
    {
        public byte[] Content { get; set; }
    }
}
