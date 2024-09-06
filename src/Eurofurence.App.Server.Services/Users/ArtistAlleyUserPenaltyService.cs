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

        public async Task SetUserPenaltyAsync(String id, ClaimsPrincipal user, ArtistAlleyUserPenaltyRecord.UserPenalties penalties, String reason)
        {
            var response = await _appDbContext.ArtistAlleyUserPenalties
                .Include(x => x.AuditLog)
                .FirstOrDefaultAsync(x => x.UserId == id);

            var oldStatus = ArtistAlleyUserPenaltyRecord.UserPenalties.OK;

            if (response is null)
            {
                response = new ArtistAlleyUserPenaltyRecord();
                response.UserId = id;
                _appDbContext.ArtistAlleyUserPenalties.Add(response);
            }
            else
            {
                oldStatus = response.Penalty;
            }

            ArtistAlleyUserPenaltyChangedRecord log = new()
            {
                ChangedBy = user.Identity?.Name,
                ChangedDateTimeUtc = DateTime.UtcNow,
                UserPenaltyRecordId = response.Id,
                NewPenalties = penalties,
                OldPenalties = oldStatus,
                Reason = reason
            };
            _appDbContext.ArtistAlleyUserPenaltiesChanges.Add(log);
            
            response.Penalty = penalties;
            await _appDbContext.SaveChangesAsync();
        }
        public async Task<ArtistAlleyUserPenaltyRecord.UserPenalties> GetUserPenaltyAsync(string id)
        {
            ArtistAlleyUserPenaltyRecord result = await _appDbContext.ArtistAlleyUserPenalties.AsNoTracking()
                .Where(x => x.UserId == id.ToString())
                .FirstOrDefaultAsync();
            if (result == null)
            {
                return ArtistAlleyUserPenaltyRecord.UserPenalties.OK;
            }
            return result.Penalty;
        }

    }
}