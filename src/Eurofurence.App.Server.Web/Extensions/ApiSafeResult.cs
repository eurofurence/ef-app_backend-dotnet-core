namespace Eurofurence.App.Server.Web.Extensions
{
    public class ApiSafeResult
    {
        public bool IsSuccessful { get; set; }
        public ApiErrorResult Error { get; set; }
    }

    public class ApiSafeResult<T> : ApiSafeResult
    {
        public T Result { get; set; }
    }
}