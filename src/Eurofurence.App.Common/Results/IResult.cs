namespace Eurofurence.App.Common.Results
{
    public interface IResult
    {
        bool IsSuccessful { get; set; }
        string ErrorMessage { get; set; }
        string ErrorCode { get; set; }
    }

    public interface IResult<T> : IResult
    {
        T Value { get; set; }
    }
}