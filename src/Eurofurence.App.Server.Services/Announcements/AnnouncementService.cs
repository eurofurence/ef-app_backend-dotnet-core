using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Announcements
{
    public class AnnouncementService : EntityServiceBase<AnnouncementRecord, AnnouncementResponse>,
        IAnnouncementService
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpContext _httpContext;
        private readonly IIdentityService _identityService;

        public AnnouncementService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IHttpContextAccessor httpContextAccessor,
            IIdentityService identityService
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _httpContext = httpContextAccessor.HttpContext;
            _identityService = identityService;
        }

        public override async Task<AnnouncementRecord> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await FindAll()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<AnnouncementRecord> FindAll()
        {
            IEnumerable<string> userRoles = new List<string>();

            if (_httpContext.User.Identity is ClaimsIdentity identity)
            {
                userRoles = _identityService.GetUserRoles(identity);
            }

            return _appDbContext.Announcements
                .Where(entity => entity.Roles == null || !entity.Roles.Any() || entity.Roles.Any(role => userRoles.Contains(role)))
                .AsNoTracking();
        }

        public override async Task<DeltaResponse<AnnouncementResponse>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<string> userRoles = new List<string>();

            if (_httpContext.User.Identity is ClaimsIdentity identity)
            {
                userRoles = _identityService.GetUserRoles(identity);
            }

            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<AnnouncementResponse>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.Announcements
                        .Where(entity =>
                            entity.IsDeleted == 0
                            && (entity.Roles == null || !entity.Roles.Any() || entity.Roles.Any(role => userRoles.Contains(role))))
                        .Select(x => x.Transform()).ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.Announcements
                    .IgnoreQueryFilters()
                    .Where(entity =>
                        entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc
                        && (entity.Roles == null || !entity.Roles.Any() || entity.Roles.Any(role => userRoles.Contains(role))));

                response.ChangedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .Select(x => x.Transform())
                    .ToArrayAsync(cancellationToken);
                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }
    }
}