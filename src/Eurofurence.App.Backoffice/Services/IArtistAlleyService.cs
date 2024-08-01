using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IArtistAlleyService
    {
        public Task<TableRegistrationResponse[]> GetTableRegistrationsAsync();
    }
}
