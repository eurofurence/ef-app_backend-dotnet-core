using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Identity;

public class SingleUseTokenPayload
{
    /// <summary>
    /// Name of the entity (usually user) behind this token.
    /// </summary>
    public string PrincipalName { get; init; }
    /// <summary>
    /// Token will be rejected after this point in time.
    /// </summary>
    public DateTimeOffset ValidUntil { get; init; }
    /// <summary>
    /// Roles this token grants.
    /// </summary>
    public IList<string> Roles { get; init; }
    /// <summary>
    /// Additional claims that may be required by the endpoint this token is intended for (e.g. "avatar").
    /// </summary>
    public IDictionary<string, string> AdditionalClaims { get; init; }
}