using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public class ConventionSettings
    {
        public string ConventionIdentifier { get; set; }
        public int ConventionNumber { get; set; }
        public string State { get; set; }
        public bool IsRegSysAuthenticationEnabled { get; set; }
        public string BaseUrl { get; set; }
        public string AppIdITunes { get; set; }
        public string AppIdPlay { get; set; }
        public string ApiBaseUrl => $"{BaseUrl}/Api";
        public string ContentBaseUrl => $"{BaseUrl}";
        public string WebBaseUrl => $"{BaseUrl}/Web";
        public string WorkingDirectory { get; set; }

        public static ConventionSettings FromConfiguration(IConfiguration configuration)
            => new ConventionSettings()
            {
                ConventionIdentifier = configuration["global:conventionIdentifier"],
                ConventionNumber = Convert.ToInt32(configuration["global:conventionNumber"]),
                State = configuration["global:state"],
                IsRegSysAuthenticationEnabled = Convert.ToInt32(configuration["global:regSysAuthenticationEnabled"]) == 1,
                BaseUrl = configuration["global:baseUrl"],
                AppIdITunes = configuration["global:appIdITunes"],
                AppIdPlay = configuration["global:appIdPlay"],
                WorkingDirectory = configuration["global:workingDirectory"]
            };
    }
}