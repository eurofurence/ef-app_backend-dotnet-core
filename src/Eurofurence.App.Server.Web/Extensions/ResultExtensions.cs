using Eurofurence.App.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Extensions
{
    public static class ResultExtensions
    {
        public static T Transient404<T>(this T obj, HttpContext context)
        {
            if (obj == null)
                context.Response.StatusCode = StatusCodes.Status404NotFound;

            return obj;
        }

        public static T Transient403<T>(this T obj, HttpContext context)
        {
            if (obj == null)
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return obj;
        }

        public static ActionResult AsActionResult(this Eurofurence.App.Common.Results.IResult obj)
        {
            if (obj.IsSuccessful) return new NoContentResult();
            return new BadRequestObjectResult(new ApiErrorResult { Code = obj.ErrorCode, Message = obj.ErrorMessage });
        }

        public static ActionResult AsActionResult<T>(this IResult<T> obj)
        {
            if (obj.IsSuccessful) return new ObjectResult(obj.Value) {StatusCode = 200};
            return new BadRequestObjectResult(new ApiErrorResult { Code = obj.ErrorCode, Message = obj.ErrorMessage });
        }

        public static ActionResult AsActionResultSafeVariant(this Eurofurence.App.Common.Results.IResult obj)
        {
            var result = new ApiSafeResult()
            {
                IsSuccessful = obj.IsSuccessful,
                Error = obj.IsSuccessful ? null : new ApiErrorResult {Code = obj.ErrorCode, Message = obj.ErrorMessage}
            };

            return new ObjectResult(result);
        }

        public static ActionResult AsActionResultSafeVariant<T>(this IResult<T> obj)
        {
            var result = new ApiSafeResult<T>();
            if (obj.IsSuccessful)
            {
                result.IsSuccessful = true;
                result.Result = obj.Value;
            }
            else
            {
                result.Error = new ApiErrorResult {Code = obj.ErrorCode, Message = obj.ErrorMessage};
            }
            return new ObjectResult(result);
        }
    }
}