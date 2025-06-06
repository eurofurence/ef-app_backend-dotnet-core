using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.Announcements
{
    public interface IAnnouncementService :
        IEntityServiceOperations<AnnouncementRecord, AnnouncementResponse>,
        IPatchOperationProcessor<AnnouncementRecord>
    {
    }
}