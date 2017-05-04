using System;

namespace Eurofurence.App.Server.Services.Security
{
    public class AuthenticationSettings
    {
        public int ConventionNumber { get; set; }
        public TimeSpan DefaultTokenLifeTime { get; set; }
    }
}
