using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public interface IAgentClosingResultService
    {
        Task<ImportResult> ImportAgentClosingResultLogAsync(TextReader logReader);

        Task ExecuteNotificationRunAsync();

        Task<IList<AgentClosingNotificationResult>> SimulateNotificationRunAsync();

        Task DeleteUnprocessedImportRowsAsync();
    }
}
