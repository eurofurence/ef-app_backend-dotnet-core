namespace Eurofurence.App.Server.Web.Controllers.Transformer;

/// <summary>
/// A transformer which is used to interact with a dto or of type <typeparamref name="TDestination"/>.
///
/// This may be used to convert a record to a dto and vice versa.
/// Furthermore, objects of <typeparamref name="TDestination"/> may be merged in the current instance.
/// </summary>
/// <typeparam name="TDestination">The destination type</typeparam>
public interface IDtoTransformer<out TDestination>
{
    /// <summary>
    /// Transforms the current class to an instance of <typeparam name="TDestination"></typeparam>.
    ///
    /// Note that some data may be lost in that process.
    /// </summary>
    /// <returns></returns>
    public TDestination Transform();



}