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
            if (options.Value.Attendee.Contains(claim.Value))
            {
                roles.Add("Attendee");
            }
            
            if (options.Value.Admin.Contains(claim.Value))
            {
                roles.Add("Admin");
            }
            
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
            
            if (options.Value.FursuitBadgeSystem.Contains(claim.Value))
            {
                roles.Add("FursuitBadgeSystem");
            }
            
            if (options.Value.PrivateMessagesSend.Contains(claim.Value))
            {
                roles.Add("Action-PrivateMessages-Send");
            }
            
            if (options.Value.PrivateMessagesQuery.Contains(claim.Value))
            {
                roles.Add("Action-PrivateMessages-Query");
            }
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }


        return Task.FromResult(principal);
    }
}