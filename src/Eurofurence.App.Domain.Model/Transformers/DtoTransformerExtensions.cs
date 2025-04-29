using Eurofurence.App.Domain.Model.Transformers;
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

    public static TDestination Transform<TDestination>(this IDtoTransformable<TDestination> transformable,
        TypeAdapterConfig configuration)
        where TDestination : class
    {
        return transformable.Transform(configuration);
    }


    /// <summary>
    /// Performs a transformer merge action with the passed <paramref name="transformable"/>
    /// </summary>
    /// <param name="transformable">The Transformer which should be used for this dto transformation</param>
    /// <param name="source">The source object which fields should be copied into the <paramref name="transformable"/> object</param>
    /// <typeparam name="TRecord">The type of object which should be merged</typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public static void Merge<TRequest, TResponse, TRecord>(
        this IDtoRecordTransformable<TRequest, TResponse, TRecord> transformable,
        TRequest source)
        where TRequest : class, IDtoTransformable<TRecord>
        where TRecord : class
        where TResponse : class
    {
        transformable.MergeDto(source);
    }

    public static void Merge<TRequest, TResponse, TRecord>(
        this IDtoRecordTransformable<TRequest, TResponse, TRecord> transformable,
        TRequest source, TypeAdapterConfig configuration)
        where TRequest : class, IDtoTransformable<TRecord>
        where TRecord : class
        where TResponse : class
    {
        transformable.MergeDto(source, configuration);
    }
}