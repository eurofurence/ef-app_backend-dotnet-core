using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Identity;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class RolesClaimsTransformation(
    IOptionsSnapshot<AuthorizationOptions> authorizationOptions,
    IIdentityService identityService
) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return principal;
        }

        if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
        {
            return principal;
        }

        await identityService.ReadUserInfo(identity);

        await identityService.ReadRegSys(identity);

        var roles = new HashSet<string>();

        foreach (var claim in identity.Claims.Where(x => x.Type == "groups"))
        {
            if (authorizationOptions.Value.Admin.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.Admin);
            }

            if (authorizationOptions.Value.ArtistAlleyAdmin.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.ArtistAlleyAdmin);
            }

            if (authorizationOptions.Value.ArtistAlleyModerator.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.ArtistAlleyModerator);
            }

            if (authorizationOptions.Value.ArtShow.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.ArtShow);
            }

            if (authorizationOptions.Value.Attendee.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.Attendee);
            }

            if (authorizationOptions.Value.AttendeeCheckedIn.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.AttendeeCheckedIn);
            }

            if (authorizationOptions.Value.EventFeedbackManager.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.EventFeedbackManager);
            }

            if (authorizationOptions.Value.KnowledgeBaseEditor.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.KnowledgeBaseEditor);
            }

            if (authorizationOptions.Value.MapEditor.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.MapEditor);
            }

            if (authorizationOptions.Value.PrivateMessageSender.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.PrivateMessageSender);
            }

            if (authorizationOptions.Value.Staff.Contains(claim.Value))
            {
                roles.Add(IdentityRoles.Staff);
            }
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }

        return principal;
    }
}