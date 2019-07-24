using System;
using Eurofurence.App.Common.Utility;

namespace Eurofurence.App.Server.Services.ArtShow
{
    class AgentClosingResultImportRow
    {
        public int AgentBadgeNo { get; set; }
        public string AgentName { get; set; }
        public string ArtistName { get; set; }
        public decimal TotalCashAmount { get; set; }
        public int ExhibitsSold { get; set; }
        public int ExhibitsUnsold { get; set; }
        public int ExhibitsToAuction { get; set; }

        public Lazy<string> Hash => new Lazy<string>(
            () => Hashing.ComputeHashSha1(AgentBadgeNo, AgentName, ArtistName, TotalCashAmount, ExhibitsSold, ExhibitsUnsold, ExhibitsToAuction));
    }
}