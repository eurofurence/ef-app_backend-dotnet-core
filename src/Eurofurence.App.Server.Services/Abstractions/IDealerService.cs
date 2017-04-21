using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IDealerService :
        IEntityServiceOperations<DealerRecord>,
        IPatchOperationProcessor<DealerRecord>
    {

    }
}