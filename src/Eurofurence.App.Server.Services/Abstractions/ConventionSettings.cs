namespace Eurofurence.App.Server.Services.Abstractions
{
    public class ConventionSettings
    {
        public string ConventionIdentifier { get; set; }
        public bool IsRegSysAuthenticationEnabled { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}