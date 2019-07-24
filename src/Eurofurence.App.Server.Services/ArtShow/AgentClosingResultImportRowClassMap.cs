using CsvHelper.Configuration;

namespace Eurofurence.App.Server.Services.ArtShow
{
    internal sealed class AgentClosingResultImportRowClassMap : ClassMap<AgentClosingResultImportRow>
    {
        public AgentClosingResultImportRowClassMap()
        {
            Map(m => m.AgentBadgeNo).Index(0);
            Map(m => m.AgentName).Index(1);
            Map(m => m.ArtistName).Index(2);
            Map(m => m.TotalCashAmount).Index(3);
            Map(m => m.ExhibitsSold).Index(4);
            Map(m => m.ExhibitsUnsold).Index(5);
            Map(m => m.ExhibitsToAuction).Index(6);
        }
    }
}