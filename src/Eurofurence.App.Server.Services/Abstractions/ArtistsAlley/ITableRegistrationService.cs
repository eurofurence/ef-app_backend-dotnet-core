﻿using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public interface ITableRegistrationService :
        IEntityServiceOperations<TableRegistrationRecord>,
        IPatchOperationProcessor<TableRegistrationRecord>
    {
        Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request, Stream imageStream);
        IQueryable<TableRegistrationRecord> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state);
        Task<TableRegistrationRecord> GetLatestRegistrationByUidAsync(string uid);

        Task ApproveByIdAsync(Guid id, string operatorUid);
        Task RejectByIdAsync(Guid id, string operatorUid);

        /// <summary>
        /// Changes the status of an applicant (e.g. banning them)
        /// </summary>
        /// <param name="id">Guid of the user</param>
        /// <param name="status">The status that should be set</param>
        /// <returns></returns>
        Task SetUserStatusAsync(Guid id, ArtistAlleyUserStatusRecord.UserStatus status);
    }
}