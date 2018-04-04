using Eurofurence.App.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class CustomValidationAttributesFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var methodParameters = context.ActionDescriptor.Parameters
                .Cast<ControllerParameterDescriptor>();

            var notNullParameters = methodParameters
                .Where(a => a.ParameterInfo.CustomAttributes.Any(b => b.AttributeType == typeof(EnsureNotNullAttribute)))
                .Select(a => a.Name);

            foreach (var propertyName in notNullParameters)
            {
                if (!context.ActionArguments.ContainsKey(propertyName) || context.ActionArguments[propertyName] == null)
                {
                    context.Result = new BadRequestObjectResult($"Unable to parse {propertyName}.");
                    return;
                }
            }


            var entityIdMatches = methodParameters
                .Where(a => a.ParameterInfo.CustomAttributes.Any(b => b.AttributeType == typeof(EnsureEntityIdMatchesAttribute)))
                .Select(a => new Tuple<string, string>(a.Name, a.ParameterInfo.CustomAttributes
                    .SingleOrDefault(b => b.AttributeType == typeof(EnsureEntityIdMatchesAttribute))
                    .ConstructorArguments[0].Value.ToString()));

            foreach(var check in entityIdMatches)
            {
                if (
                    !context.ActionArguments.ContainsKey(check.Item1) ||
                    !context.ActionArguments.ContainsKey(check.Item2) ||
                    !(context.ActionArguments[check.Item1] is IEntityBase) ||
                    !(context.ActionArguments[check.Item2] is Guid) ||
                    (((IEntityBase)context.ActionArguments[check.Item1]).Id != (Guid)context.ActionArguments[check.Item2]))
                {
                    context.Result = new BadRequestObjectResult($"{check.Item1}.Id does not match {check.Item2}");
                    return;
                }
            }

            await next();
        }
    }
}
