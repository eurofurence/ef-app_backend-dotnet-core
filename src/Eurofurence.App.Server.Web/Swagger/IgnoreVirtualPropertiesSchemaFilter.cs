using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class IgnoreVirtualPropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            //var type = context.SystemType;

            //var virtualPropertyNames =
            //    type.GetProperties()
            //        .Where(p => p.GetMethod.IsVirtual)
            //        .Select(p => p.Name)
            //        .ToArray();


            //if (model.Properties == null) return;

            //foreach (var propertyName in model.Properties.Where
            //    (p => virtualPropertyNames.Contains(p.Key, StringComparer.OrdinalIgnoreCase)).ToArray())
            //{
            //    model.Properties.Remove(propertyName);
            //}
        }
    }
}