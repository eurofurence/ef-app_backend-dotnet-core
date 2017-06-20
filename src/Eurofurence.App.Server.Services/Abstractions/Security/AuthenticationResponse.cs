using System;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class AuthenticationResponse
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidUntil { get; set; }
    }
}