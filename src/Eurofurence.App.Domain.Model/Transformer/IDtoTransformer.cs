namespace Eurofurence.App.Server.Web.Controllers.Transformer;

/// <summary>
/// A transformer which is used to interact with a dto or of type <typeparamref name="TDestination"/>.
///
/// This may be used to convert a record to a dto and vice versa.
/// Furthermore, objects of <typeparamref name="TDestination"/> may be merged in the current instance.
/// </summary>
/// <typeparam name="TDestination">The destination type</typeparam>
public interface IDtoTransformer<TDestination>
{
    /// <summary>
    /// Transforms the current class to an instance of <typeparam name="TDestination"></typeparam>.
    ///
    /// Note that some data may be lost in that process.
    /// </summary>
    /// <returns></returns>
    TDestination Transform();

    /// <summary>
    /// Merges the data from <paramref name="source"/> into the current instance.
    ///
    /// This method should override all non-null fields of <paramref name="source"/>
    ///
    /// Note that not all data may be affected from that.
    /// </summary>
    /// <param name="source">An object (likely a DTO or record) whose data will be applied.</param>
    void Merge(TDestination source);
}