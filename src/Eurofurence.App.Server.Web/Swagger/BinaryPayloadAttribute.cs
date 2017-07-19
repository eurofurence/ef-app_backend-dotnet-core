using System;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class BinaryPayloadAttribute : Attribute
    {
        public BinaryPayloadAttribute()
        {
            ParameterName = "Payload";
            Required = true;
            MediaType = "application/octet-stream";
        }

        public string Description { get; set; }

        public string MediaType { get; set; }

        public bool Required { get; set; }

        public string ParameterName { get; set; }
    }
}
