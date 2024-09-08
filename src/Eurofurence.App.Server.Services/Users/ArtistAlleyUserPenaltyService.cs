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
    public class ArtistAlleyUserPenaltyService : EntityServiceBase<ArtistAlleyUserPenaltyRecord>, IArtistAlleyUserPenaltyService
    {

        private readonly AppDbContext _appDbContext;
        public ArtistAlleyUserPenaltyService(AppDbContext appDbContext, IStorageServiceFactory storageServiceFactory, Boolean useSoftDelete = true) : base(appDbContext, storageServiceFactory, useSoftDelete)
        {
            _appDbContext = appDbContext;
        }

        public async Task SetUserPenaltyAsync(String id, ClaimsPrincipal user, ArtistAlleyUserPenaltyRecord.PenaltyStatus penalties, String reason)
        {
            var response = await _appDbContext.ArtistAlleyUserPenalties
                .Include(x => x.AuditLog)
                .FirstOrDefaultAsync(x => x.IdentityId == id);

            var oldStatus = ArtistAlleyUserPenaltyRecord.PenaltyStatus.OK;

            if (response is null)
            {
                response = new ArtistAlleyUserPenaltyRecord();
                response.IdentityId = id;
                _appDbContext.ArtistAlleyUserPenalties.Add(response);
            }
            else
            {
                oldStatus = response.Status;
            }

            ArtistAlleyUserPenaltyRecord.StateChangeRecord log = new()
            {
                ChangedBy = user.Identity?.Name,
                UserPenaltyRecordId = response.Id,
                PenaltyStatus = penalties,
                Reason = reason
            };
            _appDbContext.ArtistAlleyUserPenaltyChanges.Add(log);
            
            response.Status = penalties;
            await _appDbContext.SaveChangesAsync();
        }
        public async Task<ArtistAlleyUserPenaltyRecord.PenaltyStatus> GetUserPenaltyAsync(string id)
        {
            ArtistAlleyUserPenaltyRecord result = await _appDbContext.ArtistAlleyUserPenalties.AsNoTracking()
                .Where(x => x.IdentityId == id.ToString())
                .FirstOrDefaultAsync();
            if (result == null)
            {
                return ArtistAlleyUserPenaltyRecord.PenaltyStatus.OK;
            }
            return result.Status;
        }

    }
}