using Eurofurence.App.Domain.Model.Maps;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IMapService :
    IEntityServiceOperations<MapRecord>,
    IPatchOperationProcessor<MapRecord>
    {

    }
}