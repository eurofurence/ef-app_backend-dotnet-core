using Eurofurence.App.Domain.Model.LostAndFound;

namespace Eurofurence.App.Server.Services.Abstractions.LostAndFound
{
    public interface ILostAndFoundService :
        IEntityServiceOperations<LostAndFoundRecord>,
        IPatchOperationProcessor<LostAndFoundRecord>
    {
    }
}