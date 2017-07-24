using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class BinaryPayloadFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            var attribute =
            (apiDescription.ActionAttributes()
                .FirstOrDefault(a => a is BinaryPayloadAttribute) as BinaryPayloadAttribute);

            if (attribute == null)
            {
                return;
            }

            operation.Consumes.Clear();
            operation.Consumes.Add(attribute.MediaType);

            operation.Parameters.Add(new BodyParameter()
            {
                Name = attribute.ParameterName,
                In = "body",
                Required = attribute.Required,
                Description = attribute.Description,
                Schema = new Schema()
                {
                    Type = "string",
                    Format = "byte"
                }
            });
        }
    }
}