namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class RegSysAuthenticationRequest
    {
        public int RegNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}