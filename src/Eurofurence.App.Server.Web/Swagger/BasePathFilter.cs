using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class BasePathFilter : IDocumentFilter
    {
        private string _basePath;

        public BasePathFilter(string basePath)
        {
            _basePath = basePath;
        }
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.BasePath = _basePath;
        }
    }
}
