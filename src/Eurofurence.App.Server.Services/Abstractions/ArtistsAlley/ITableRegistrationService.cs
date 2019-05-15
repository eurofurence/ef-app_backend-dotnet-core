using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public interface ITableRegistrationService
    {
        Task RegisterTableAsync(string uid, TableRegistrationRequest request);
        Task<IEnumerable<TableRegistrationRecord>> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state);
        Task<TableRegistrationRecord> GetLatestRegistrationByUid(string uid);

        Task ApproveByIdAsync(Guid id, string operatorUid);
        Task RejectByIdAsync(Guid id, string operatorUid);
    }
}
