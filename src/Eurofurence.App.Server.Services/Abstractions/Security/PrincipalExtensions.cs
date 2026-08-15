using System.Security.Claims;

namespace Eurofurence.App.Server.Services.Abstractions.Security;

public static class PrincipalExtensions
{
    public static string GetSubject(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("sub");
    }

    public static string GetRegSysId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("RegSysId");
    }

    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("name");
    }
}