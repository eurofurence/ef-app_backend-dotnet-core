using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public interface ITableRegistrationService :
        IEntityServiceOperations<TableRegistrationRecord, TableRegistrationResponse>,
        IPatchOperationProcessor<TableRegistrationRecord>
    {
        Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request, Stream imageStream);
        IQueryable<TableRegistrationRecord> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state);
        Task<TableRegistrationRecord> GetLatestRegistrationByUidAsync(string uid);

        /// <summary>
        /// Deletes the latest registration of a given user by their ID (<paramref name="uid"/>)
        /// </summary>
        /// <param name="uid">The ID of the user whose registration should be deleted</param>
        /// <exception cref="ArgumentException">Can be thrown when no registration can be found under the users <paramref name="uid"/></exception>
        /// <returns></returns>
        Task DeleteLatestRegistrationByUidAsync(string uid);

        Task ApproveByIdAsync(Guid id, string operatorUid);
        Task RejectByIdAsync(Guid id, string operatorUid);
    }
}