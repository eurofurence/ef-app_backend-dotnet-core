using System.Collections.Generic;
using System.Linq;
using Autofac;
using Eurofurence.App.Server.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eurofurence.App.Server.Web.Swagger
{
    public class AddAuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            var methodInfo =
                (apiDescription.ActionDescriptor as ControllerActionDescriptor)?
                .MethodInfo;

            if (methodInfo == null ||
                methodInfo.CustomAttributes.All(a => !a.AttributeType.IsAssignableTo<AuthorizeAttribute>()))
                return;


            var requiredRoles = methodInfo.CustomAttributes
                .Where(a => a.AttributeType.IsAssignableTo<AuthorizeAttribute>())
                .SelectMany(a => a.NamedArguments.Where(b => b.MemberName == "Roles"))
                .Select(a => a.TypedValue.Value.ToString())
                .SelectMany(a => a.Split(',', ';', ' '))
                .Select(a => a.Trim())
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            operation.Security = new List<OpenApiSecurityRequirement>();

            var oAuthRequirements =
               new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                };

            var apiKeyRequirements =
               new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ApiKeyAuthenticationDefaults.AuthenticationScheme
                            }
                        },
                        new List<string>()
                    }
                };

            operation.Security.Add(oAuthRequirements);
            operation.Security.Add(apiKeyRequirements);
            operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();

            var existingDescription = operation.Description;

            operation.Description = "  * Requires authorization  \n";

            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse {Description = "Authorization required"});

            if (requiredRoles.Count > 0)
            {
                operation.Description += "  * Requires any of the following roles: "
                                         + string.Join(", ", requiredRoles.Select(a => $"**`{a}`**"));

                if (!operation.Responses.ContainsKey("403"))
                    operation.Responses.Add("403", new OpenApiResponse
                    {
                        Description = "Authorization not sufficient (Missing Role)  \n"
                                      + string.Join("\n", requiredRoles.Select(a => $"  * Not in role **`{a}`**"))
                    });
            }

            if (!string.IsNullOrWhiteSpace(existingDescription))
                operation.Description += "\n\n" + existingDescription;
        }
    }
}