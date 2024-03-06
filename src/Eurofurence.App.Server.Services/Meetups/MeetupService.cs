using Eurofurence.App.Server.Services.Abstractions.Meetups;
using Eurofurence.App.Domain.Model.Meetups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Meetups
{
    public class MeetupService : EntityServiceBase<Meetup>, IMeetupService
    {
        public MeetupService(IEntityRepository<Meetup> entityRepository, 
            IStorageServiceFactory storageServiceFactory, 
            bool useSoftDelete = true) : base(entityRepository, storageServiceFactory, useSoftDelete)
        {
        }
    }
}
