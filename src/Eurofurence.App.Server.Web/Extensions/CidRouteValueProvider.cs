using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class CidRouteBaseAttribute : Attribute, IRouteValueProvider
    {
        internal static string Value { get; set; } = string.Empty;

        public string RouteKey => "cid";
        public string RouteValue => Value;
    }
}
