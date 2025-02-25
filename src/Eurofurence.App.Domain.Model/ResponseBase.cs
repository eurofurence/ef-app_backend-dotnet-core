using System;

namespace Eurofurence.App.Domain.Model;

public record ResponseBase()
{
    public Guid Id { get; init; }

}