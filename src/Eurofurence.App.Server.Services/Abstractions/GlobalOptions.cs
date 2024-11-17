namespace Eurofurence.App.Server.Services.Abstractions
{
    public class GlobalOptions
    {
        public string ConventionIdentifier { get; set; }
        public int ConventionNumber { get; set; }
        public string State { get; set; }
        public string BaseUrl { get; set; }
        public string AppIdITunes { get; set; }
        public string AppIdPlay { get; set; }
        public string ApiBaseUrl => $"{BaseUrl}/Api";
        public string ContentBaseUrl => $"{BaseUrl}";
        public string WebBaseUrl => $"{BaseUrl}/Web";
        public string WorkingDirectory { get; set; }
    }
}