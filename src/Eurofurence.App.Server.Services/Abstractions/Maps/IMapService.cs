using Eurofurence.App.Domain.Model.Maps;

namespace Eurofurence.App.Server.Services.Abstractions.Maps
{
    public interface IMapService :
        IEntityServiceOperations<MapRecord>,
        IPatchOperationProcessor<MapRecord>
    {
    }
}