using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public interface ITableRegistrationService
    {
        Task RegisterTableAsync(string uid, TableRegistrationRequest request);
    }
}
