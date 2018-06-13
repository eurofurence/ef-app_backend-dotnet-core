namespace Eurofurence.App.Server.Services.Abstractions
{
    public class ConventionSettings
    {
        public int ConventionNumber { get; set; }
        public bool IsRegSysAuthenticationEnabled { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}