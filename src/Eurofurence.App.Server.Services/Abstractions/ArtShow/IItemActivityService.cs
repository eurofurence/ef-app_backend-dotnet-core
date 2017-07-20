using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public interface IItemActivityService
    {
        Task ImportActivityLogAsync(TextReader logReader);

        Task<IList<NotificationResult>> SimulateNotificationRunAsync();
        Task ExecuteNotificationRunAsync();
    }

    public class NotificationResult
    {
        public string RecipientUid { get; set; }
        public IList<int> IdsSold { get; set; }
        public IList<int> IdsToAuction { get; set; }
    }

}
