using System;
using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Identity;

public class SingleUseToken
{
    public string PrincipalName { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
    public string Token { get; init; }
    public IList<string> Roles { get; init; }
    public IDictionary<string, string> AdditionalClaims { get; init; }
}