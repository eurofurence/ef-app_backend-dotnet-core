using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Users
{
    public class ArtistAlleyUserStatusService : EntityServiceBase<ArtistAlleyUserStatusRecord>, IArtistAlleyUserStatusService
    {

        private readonly AppDbContext _appDbContext;
        public ArtistAlleyUserStatusService(AppDbContext appDbContext, IStorageServiceFactory storageServiceFactory, Boolean useSoftDelete = true) : base(appDbContext, storageServiceFactory, useSoftDelete)
        {
            _appDbContext = appDbContext;
        }

        public async Task SetUserStatusAsync(String id, ClaimsPrincipal user, ArtistAlleyUserStatusRecord.UserStatus status, String reason)
        {
            var response = await _appDbContext.ArtistAlleyUserStatus
                .Include(x => x.AuditLog)
                .FirstOrDefaultAsync(x => x.UserId == id);

            var oldStatus = ArtistAlleyUserStatusRecord.UserStatus.OK;

            if (response is null)
            {
                response = new ArtistAlleyUserStatusRecord();
                response.UserId = id;
                _appDbContext.ArtistAlleyUserStatus.Add(response);
            }
            else
            {
                oldStatus = response.Status;
            }

            ArtistAlleyUserStatusChangedRecord log = new()
            {
                ChangedBy = user.Identity?.Name,
                ChangedDateTimeUtc = DateTime.UtcNow,
                UserStatusRecordID = response.Id,
                NewStatus = status,
                OldStatus = oldStatus,
                Reason = reason
            };
            _appDbContext.ArtistAlleyUserStatusChanged.Add(log);
            
            response.Status = status;
            //response.AuditLog.Add(log);
            await _appDbContext.SaveChangesAsync();
        }
        public async Task<ArtistAlleyUserStatusRecord.UserStatus> GetUserStatusAsync(string id)
        {
            ArtistAlleyUserStatusRecord result = await _appDbContext.ArtistAlleyUserStatus.AsNoTracking()
                .Where(x => x.UserId == id.ToString())
                .FirstOrDefaultAsync();
            if (result == null)
            {
                return ArtistAlleyUserStatusRecord.UserStatus.OK;
            }
            return result.Status;
        }

    }
}