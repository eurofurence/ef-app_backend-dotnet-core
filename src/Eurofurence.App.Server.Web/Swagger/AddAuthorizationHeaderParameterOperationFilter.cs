using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

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


            var requiredRoles = methodInfo.CustomAttributes
                .Where(a => a.AttributeType.IsAssignableTo<AuthorizeAttribute>())
                .SelectMany(a => a.NamedArguments.Where(b => b.MemberName == "Roles"))
                .Select(a => a.TypedValue.Value.ToString())
                .SelectMany(a => a.Split(new char[] { ',', ';', ' ' }))
                .Select(a => a.Trim())
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

            if (operation.Security == null)
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new List<string>() { } }
                };

            operation.Security.Add(oAuthRequirements);
            operation.Parameters = operation.Parameters ?? new List<IParameter>();

            var existingDescription = operation.Description;

            operation.Description = "  * Requires authorization  \n";

            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new Response() { Description = "Authorization required" });
            }

            if (requiredRoles.Count > 0)
            {
                operation.Description += "  * Requires any of the following roles: "
                    + string.Join(", ", requiredRoles.Select(a => $"**`{a}`**"));

                if (!operation.Responses.ContainsKey("403"))
                {
                    operation.Responses.Add("403", new Response() {
                        Description = "Authorization not sufficient (Missing Role)  \n"
                            + string.Join("\n", requiredRoles.Select(a => $"  * Not in role **`{a}`**"))
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(existingDescription))
            {
                operation.Description += "\n\n" + existingDescription;
            }
        }
    }
   
}
