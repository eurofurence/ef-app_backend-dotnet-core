using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class BinaryPayloadFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
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