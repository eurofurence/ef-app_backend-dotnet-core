using System.Security.Claims;

namespace Eurofurence.App.Server.Services.Abstractions.Security;

public static class PrincipalExtensions
{
    public static string GetSubject(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("sub")?.Value;
    }

    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("name")?.Value;
    }
}