using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Identity;

public class SingleUseTokenPayload
{
    public string PrincipalName { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
    public IList<string> Roles { get; init; }
    public IDictionary<string, string> AdditionalClaims { get; init; }
}