using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class CalendarAuthenticationOptions() : AuthenticationSchemeOptions
{
}

public class CalendarAuthentificationHandler : AuthenticationHandler<CalendarAuthenticationOptions>
{
    private IRegistrationIdentityService _registrationIdentityService;

    public CalendarAuthentificationHandler(
        [NotNull][ItemNotNull] IOptionsMonitor<CalendarAuthenticationOptions> options, [NotNull] ILoggerFactory logger,
        [NotNull] UrlEncoder encoder, [NotNull] ISystemClock clock, IRegistrationIdentityService identityService) : base(options, logger, encoder, clock)
    {
        _registrationIdentityService = identityService;
    }

    public CalendarAuthentificationHandler(
        [NotNull][ItemNotNull] IOptionsMonitor<CalendarAuthenticationOptions> options, [NotNull] ILoggerFactory logger,
        [NotNull] UrlEncoder encoder, IRegistrationIdentityService identityService) : base(options, logger, encoder)
    {
        _registrationIdentityService = identityService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string requestApiKey = Context.Request.Headers[ApiKeyAuthenticationDefaults.HeaderName].FirstOrDefault();

        UserRecord user = await _registrationIdentityService.FindAll(record => record.CalendarToken.Equals(requestApiKey))
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }
        var claims = new List<Claim>
        {
            new Claim("sub", user.IdentityId),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}