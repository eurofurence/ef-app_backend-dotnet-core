using Mapster;

namespace Eurofurence.App.Server.Web.Controllers.Transformers;

/// <summary>
/// Extension class for working with our DtoTransformerables (<see cref="IDtoTransformable{TDestination}"/> and <see cref="IDtoRecordTransformable{TRequest,TResponse}"/>)
/// </summary>
public static class DtoTransformerExtensions
{
    /// <summary>
    /// Performs a transform with the passed <see cref="transformable"/>
    /// </summary>
    /// <param name="transformable">The Transformer which should be used for this dto transformation</param>
    /// <typeparam name="TDestination">The destination to which the transformation be run for</typeparam>
    /// <returns></returns>
    public static TDestination Transform<TDestination>(this IDtoTransformable<TDestination> transformable)
        where TDestination : class
    {
        return transformable.Transform();
    }

    /// <summary>
    /// Performs a transformer merge action with the passed <paramref name="transformable"/>
    /// </summary>
    /// <param name="transformable">The Transformer which should be used for this dto transformation</param>
    /// <param name="source">The source object which fields should be copied into the <paramref name="transformable"/> object</param>
    /// <typeparam name="TRecord">The type of object which should be merged</typeparam>
    /// <returns></returns>
    public static void Merge<TRecord>(this object transformable, TRecord source)
        where TRecord : class
    {
        //transformable.Merge(source);
        source.Adapt(transformable);
        //source.Adapt(transformable as TRecord);
    }
}