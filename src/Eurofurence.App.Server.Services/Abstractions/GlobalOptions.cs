namespace Eurofurence.App.Server.Services.Abstractions
{
    public class GlobalOptions
    {
        public string ConventionIdentifier { get; init; }
        public int ConventionNumber { get; init; }
        public string State { get; init; }
        public string BaseUrl { get; init; }
        public string AppIdITunes { get; init; }
        public string AppIdPlay { get; init; }
        public string ApiBaseUrl => $"{BaseUrl}/Api";
        public string ContentBaseUrl => $"{BaseUrl}";
        public string WebBaseUrl => $"{BaseUrl}/Web";
        public string WorkingDirectory { get; init; }
    }
}