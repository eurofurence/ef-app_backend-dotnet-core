using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.LostAndFound
{
    public interface ILostAndFoundLassieImporter
    {
        Task RunImportAsync();
    }
}
