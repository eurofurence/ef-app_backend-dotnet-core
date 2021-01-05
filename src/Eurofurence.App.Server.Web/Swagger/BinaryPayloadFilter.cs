using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class BinaryPayloadFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            apiDescription.TryGetMethodInfo(out var methodInfo);

            if (!(methodInfo?
                .GetCustomAttributes(false)
                .FirstOrDefault(a => a is BinaryPayloadAttribute)
                is BinaryPayloadAttribute attribute))
            {
                return;
            }

            operation.RequestBody = new OpenApiRequestBody()
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>()
                {
                    { "Content", 
                        new OpenApiMediaType()
                        {
                            Schema = new OpenApiSchema()
                            {
                                Type = "string",
                                Format = "byte"
                            }
                        }
                    }
                }
            };
        }

    }
}