using System;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class AuthenticationSettings
    {
        public TimeSpan DefaultTokenLifeTime { get; set; }
    }
}