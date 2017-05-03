using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IAnnouncementService :
        IEntityServiceOperations<AnnouncementRecord>,
        IPatchOperationProcessor<AnnouncementRecord>
    {

    }
}