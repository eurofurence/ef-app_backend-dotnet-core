using Eurofurence.App.Domain.Model.Transformers;
using Mapster;

namespace Eurofurence.App.Server.Web.Controllers.Transformers;

/// <summary>
/// Extension class for working with <see cref="IDtoTransformable{TDestination}"/> and <see cref="IDtoRecordTransformable{TRequest,TResponse,TRecord}"/>.
/// </summary>
public static class DtoTransformerExtensions
{
    /// <summary>
    /// Performs a transform with the passed <see cref="transformable"/>.
    /// </summary>
    /// <param name="transformable">The Transformer, which should be used for this dto transformation.</param>
    /// <typeparam name="TDestination">The destination to which the transformation be run for.</typeparam>
    /// <returns>The transformed object of type <see cref="TDestination"/>.</returns>
    public static TDestination Transform<TDestination>(this IDtoTransformable<TDestination> transformable)
        where TDestination : class
    {
        return transformable.Transform();
    }

    /// <summary>
    /// Performs a transform with the passed <see cref="transformable"/> and a custom configuration.
    /// </summary>
    /// <param name="transformable">The Transformer, which should be used for this dto transformation.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <typeparam name="TDestination">The destination to which the transformation be run for.</typeparam>
    /// <returns>The transformed object of type <see cref="TDestination"/>.</returns>
    public static TDestination Transform<TDestination>(this IDtoTransformable<TDestination> transformable,
        TypeAdapterConfig configuration)
        where TDestination : class
    {
        return transformable.Transform(configuration);
    }


    /// <summary>
    /// Performs a transformer merge action with the passed <paramref name="transformable"/>.
    /// </summary>
    /// <param name="transformable">The Transformer, which should be used for this dto transformation</param>
    /// <param name="source">The source object which fields should be copied into the <paramref name="transformable"/> object</param>
    /// <typeparam name="TRecord">The record type (the type to merge in to).</typeparam>
    /// <typeparam name="TRequest">The request type (the type to merge from).</typeparam>
    /// <typeparam name="TResponse">Needed to ensure the correct implementation of <see cref="IDtoRecordTransformable{TRequest,TResponse,TRecord}"/>.</typeparam>
    public static void Merge<TRequest, TResponse, TRecord>(
        this IDtoRecordTransformable<TRequest, TResponse, TRecord> transformable,
        TRequest source)
        where TRequest : class, IDtoTransformable<TRecord>
        where TRecord : class
        where TResponse : class
    {
        transformable.MergeDto(source);
    }

    /// <summary>
    /// Performs a transformer merge action with the passed <paramref name="transformable"/> and a custom configuration.
    /// </summary>
    /// <param name="transformable">The Transformer, which should be used for this dto transformation</param>
    /// <param name="source">The source object which fields should be copied into the <paramref name="transformable"/> object</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <typeparam name="TRecord">The record type (the type to merge in to).</typeparam>
    /// <typeparam name="TRequest">The request type (the type to merge from).</typeparam>
    /// <typeparam name="TResponse">Needed to ensure the correct implementation of <see cref="IDtoRecordTransformable{TRequest,TResponse,TRecord}"/>.</typeparam>
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