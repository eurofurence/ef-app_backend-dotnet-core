using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sentry;

namespace Eurofurence.App.Server.Web.Identity;

public class SentryUserFactory(IHttpContextAccessor httpContextAccessor) : ISentryUserFactory
{
    public SentryUser Create()
    {
        if (httpContextAccessor.HttpContext is not { } httpContext)
        {
            return null;
        }

        if (httpContext.User is not { } user)
        {
            return null;
        }

        var id = user.FindFirstValue("sub");
        var name = user.FindFirstValue("name");
        var email = user.FindFirstValue("email");
        var ip = httpContext.Connection?.RemoteIpAddress?.ToString();
        var roles = string.Join(',', user
            .FindAll(((ClaimsIdentity)user.Identity!).RoleClaimType)
            .Select(x => x.Value));

        if (id is null && name is null && email is null && ip is null)
        {
            return null;
        }

        return new SentryUser
        {
            Id = id,
            Username = name,
            Email = email,
            IpAddress = ip,
            Other = new Dictionary<string, string>
            {
                { "roles", roles }
            }
        };
    }
}