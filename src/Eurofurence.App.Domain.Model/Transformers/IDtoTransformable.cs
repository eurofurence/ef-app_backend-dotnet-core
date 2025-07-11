using System.Runtime.CompilerServices;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;

namespace Eurofurence.App.Server.Web.Controllers.Transformers;

/// <summary>
/// Provides a contract for transforming an object into an instance of a specified destination type.
/// Implementing types can use this interface to convert themselves into DTOs, potentially losing some data during the transformation process, when not all fields are mapped in <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TDestination">The target type to transform to.</typeparam>
public interface IDtoTransformable<out TDestination>
    where TDestination : class
{
    /// <summary>
    /// Transforms the current class to an instance of <typeparam name="TDestination"></typeparam>.
    ///
    /// Note that some data may be lost in that process.
    /// </summary>
    /// <returns></returns>
    TDestination Transform()
    {
        return ModelMapper.Map<TDestination>(this);
    }
}