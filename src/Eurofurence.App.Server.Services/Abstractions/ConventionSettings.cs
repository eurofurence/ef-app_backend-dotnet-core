using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public class ConventionSettings
    {
        public string ConventionIdentifier { get; set; }
        public bool IsRegSysAuthenticationEnabled { get; set; }
        public string ApiBaseUrl { get; set; }
        public string AppIdITunes { get; set; } 
        public string AppIdPlay { get; set; }

        public static ConventionSettings FromConfiguration(IConfiguration configuration) 
            => new ConventionSettings()
            {
                ConventionIdentifier = configuration["global:conventionIdentifier"],
                IsRegSysAuthenticationEnabled = Convert.ToInt32(configuration["global:regSysAuthenticationEnabled"]) == 1,
                ApiBaseUrl = configuration["global:apiBaseUrl"],
                AppIdITunes = configuration["global:appIdITunes"],
                AppIdPlay = configuration["global:appIdPlay"]
            };
    }
}