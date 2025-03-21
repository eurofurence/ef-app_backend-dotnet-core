using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.Transformers;

public interface IDtoRecordTransformable<in TRequest, out TResponse, TRecord> : IDtoTransformable<TResponse>
    where TRequest : class, IDtoTransformable<TRecord>
    where TResponse : class
    where TRecord : class
{
    /// <summary>
    /// Merges the data from <paramref name="source"/> into the current instance.
    ///
    /// This method should override all non-null fields of <paramref name="source"/>
    ///
    /// Note that not all data may be affected from that.
    /// </summary>
    /// <param name="source">An object (likely a DTO or record) whose data will be applied.</param>
    void MergeDto(TRequest source)
    {
        source.Adapt(this as TRecord);
    }
}