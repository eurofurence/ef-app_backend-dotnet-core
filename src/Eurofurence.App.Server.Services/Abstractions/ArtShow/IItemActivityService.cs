using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public interface IItemActivityService
    {
        Task<ImportResult> ImportActivityLogAsync(TextReader logReader);

        IList<ItemActivityNotificationResult> SimulateNotificationRun();
        Task ExecuteNotificationRunAsync();
        Task DeleteUnprocessedImportRowsAsync();
    }
}