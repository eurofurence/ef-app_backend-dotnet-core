using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class AddAuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            var methodInfo =
                (apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?
                .MethodInfo;

            if (methodInfo == null ||
                methodInfo.CustomAttributes.All(a => !a.AttributeType.IsAssignableTo<AuthorizeAttribute>()))
                return;


            operation.Parameters = operation.Parameters ?? new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter()
            {
                Name = "Authorization",
                In = "header",
                Description = "JWT bearer token.",
                Required = true,
                Type = "string"
            });
        }
    }
}
