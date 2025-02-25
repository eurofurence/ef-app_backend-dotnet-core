namespace Eurofurence.App.Server.Web.Controllers.Transformer;

public interface IDtoRecordTransformer<in TRequest, TResponse> : IDtoTransformer<TResponse>
{
    /// <summary>
    /// Merges the data from <paramref name="source"/> into the current instance.
    ///
    /// This method should override all non-null fields of <paramref name="source"/>
    ///
    /// Note that not all data may be affected from that.
    /// </summary>
    /// <param name="source">An object (likely a DTO or record) whose data will be applied.</param>
    void Merge(TRequest source);
}