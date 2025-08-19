using Mapster;

namespace Eurofurence.App.Domain.Model.Transformers;

/// <summary>
/// Defines a contract for objects that can transform DTOs into response objects,
/// providing functionality to merge DTO data into the current instance.
/// </summary>
/// <typeparam name="TRequest">The type of the incoming DTO used as the source for transformation.</typeparam>
/// <typeparam name="TResponse">The type of the response DTO produced by the transformation.</typeparam>
/// <typeparam name="TRecord">The type of the record object that receives merged data from the DTO.</typeparam>
/// <remarks>
/// Implements <see cref="IDtoTransformable{TResponse}"/> to allow transforming the current instance into a response type.
///
/// Implementing the methods is usually not necessary, as the default implementations provide the required functionality.
/// </remarks>
public interface IDtoRecordTransformable<in TRequest, out TResponse, TRecord> : IDtoTransformable<TResponse>
    where TRequest : class, IDtoTransformable<TRecord>
    where TResponse : class
    where TRecord : class
{
    /// <summary>
    /// Merges the data from <paramref name="source"/> into the current instance.
    /// </summary>
    /// <param name="source">An object whose data will be applied.</param>
    void MergeDto(TRequest source)
    {
        source.Adapt(this as TRecord);
    }

    /// <summary>
    /// Merges the data from <paramref name="source"/> into the current instance with a configuration.
    /// </summary>
    /// <param name="source">An object whose data will be applied.</param>
    /// <param name="configuration">The configuration to use.</param>
    void MergeDto(TRequest source, TypeAdapterConfig configuration)
    {
        source.Adapt(this as TRecord, config: configuration);
    }
}