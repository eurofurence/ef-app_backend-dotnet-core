using Microsoft.AspNetCore.Http;

namespace Eurofurence.App.Server.Web.Extensions
{
    public static class ResultExtensions
    {
        public static T Transient404<T>(this T obj , HttpContext context)
        {
            if (obj == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }

            return obj;
        }
    }
}
