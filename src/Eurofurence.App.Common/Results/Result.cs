namespace Eurofurence.App.Common.Results
{
    public class Result : IResult
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        
        public static explicit operator bool(Result r) => r.IsSuccessful;

        public static Result Ok => new Result {IsSuccessful = true};
        public static Result Error(string errorCode, string errorMessage = "")
            => new Result {IsSuccessful = false, ErrorCode =  errorCode, ErrorMessage = errorMessage};
    }

    public class Result<T> : Result, IResult<T>
    {
        public T Value { get; set; }
        public static explicit operator T(Result<T> r) => r.Value;

        public new static Result<T> Ok(T value) => new Result<T> {IsSuccessful = true, Value = value};
        public new static Result<T> Error(string errorCode, string errorMessage = "")
            => new Result<T> { IsSuccessful = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
    }
}
