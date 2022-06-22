using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public class AgentClosingNotificationResult
    {
        public int AgentBadgeNo { get; set; }
        public string AgentName { get; set; }
        public string ArtistName { get; set; }
        public decimal TotalCashAmount { get; set; }
        public int ExhibitsSold { get; set; }
        public int ExhibitsUnsold { get; set; }
        public int ExhibitsToAuction { get; set; }
    }
}