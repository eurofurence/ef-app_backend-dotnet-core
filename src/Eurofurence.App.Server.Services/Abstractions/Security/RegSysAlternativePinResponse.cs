namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class RegSysAlternativePinResponse
    {
        public int RegNo { get; set; }
        public string NameOnBadge { get; set; }
        public string Pin { get; set; }
    }
}