using System.IO;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public interface IItemActivityService
    {
        Task ImportActivityLogAsync(TextReader logReader);
    }
}
