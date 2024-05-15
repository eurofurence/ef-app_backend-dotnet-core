using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public interface IAgentClosingResultService
    {
        Task<ImportResult> ImportAgentClosingResultLogAsync(TextReader logReader);

        Task ExecuteNotificationRunAsync();

        IList<AgentClosingNotificationResult> SimulateNotificationRun();

        Task DeleteUnprocessedImportRowsAsync();
    }
}
