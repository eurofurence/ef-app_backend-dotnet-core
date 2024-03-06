using Eurofurence.App.Domain.Model.Meetups;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class MeetupRepository: MongoDbEntityRepositoryBase<Meetup>
    {
        public MeetupRepository(IMongoCollection<Meetup> collection)
            : base(collection)
        {
        }
    }
}
