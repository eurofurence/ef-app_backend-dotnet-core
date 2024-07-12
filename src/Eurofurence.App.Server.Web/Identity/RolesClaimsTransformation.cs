using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class RolesClaimsTransformation(IOptionsSnapshot<AuthorizationOptions> options) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        var roles = new HashSet<string>();

        foreach (var claim in identity.Claims.Where(x => x.Type == "groups"))
        {
            if (options.Value.System.Contains(claim.Value))
            {
                roles.Add("System");
            }

            if (options.Value.Developer.Contains(claim.Value))
            {
                roles.Add("Developer");
            }

            if (options.Value.KnowledgeBaseMaintainer.Contains(claim.Value))
            {
                roles.Add("KnowledgeBase-Maintainer");
            }

            if (options.Value.ArtShow.Contains(claim.Value))
            {
                roles.Add("ArtShow");
            }
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }


        return Task.FromResult(principal);
    }
}