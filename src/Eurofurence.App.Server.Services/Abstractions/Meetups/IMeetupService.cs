using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Meetups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Meetups
{
    public interface IMeetupService : 
        IEntityServiceOperations<Meetup>
        ,IPatchOperationProcessor<Meetup>
    {
    }
}
