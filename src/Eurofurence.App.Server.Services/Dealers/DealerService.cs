using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Dealers
{
    public class DealerService : EntityServiceBase<DealerRecord>,
        IDealerService
    {
        private readonly AppDbContext _appDbContext;

        public DealerService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        public override async Task ReplaceOneAsync(DealerRecord entity)
        {
            var existingEntity = await _appDbContext.Dealers
                .Include(dealerRecord => dealerRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(ke => ke.Id == entity.Id);

            foreach (var existingLink in existingEntity.Links)
            {
                if (!entity.Links.Contains(existingLink))
                {
                    _appDbContext.LinkFragments.Remove(existingLink);
                }
            }

            foreach (var link in entity.Links)
            {
                var linkExists = await _appDbContext.LinkFragments.AnyAsync(existingLink => existingLink.Id == link.Id);
                if (!linkExists)
                {
                    _appDbContext.LinkFragments.Add(link);
                }
            }
            await base.ReplaceOneAsync(entity);
        }
    }
}