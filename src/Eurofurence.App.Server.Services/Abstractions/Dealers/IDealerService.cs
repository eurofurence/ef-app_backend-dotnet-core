using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Server.Services.Abstractions.Dealers
{
    public interface IDealerService :
        IEntityServiceOperations<DealerRecord>,
        IPatchOperationProcessor<DealerRecord>
    {

    }
}