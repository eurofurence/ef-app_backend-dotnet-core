namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public int RegNo { get; set; }
        public string Username { get; set; }
        public string Source { get; set; }
    }
}