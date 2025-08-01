using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public interface ITableRegistrationService :
        IEntityServiceOperations<TableRegistrationRecord, TableRegistrationResponse>,
        IPatchOperationProcessor<TableRegistrationRecord>
    {
        Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request, Stream imageStream);

        /// <summary>
        /// Updates an existing table registration identified by the specified ID.
        ///
        /// This will set the status of the registration to back to "Pending".
        /// </summary>
        /// <param name="id">The unique identifier of the table registration to update</param>
        /// <param name="request">The request object containing updated registration details</param>
        /// <param name="imageStream">Stream containing the new image data for the table registration</param>
        /// <returns>A task representing the asynchronous update operation</returns>
        Task UpdateTableAsync(ClaimsPrincipal user, Guid id, TableRegistrationRequest request, Stream imageStream);

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
