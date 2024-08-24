using System.Security.Claims;
using Eurofurence.App.Backoffice.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace Eurofurence.App.Backoffice.Authentication
{
    public class BackendAccountClaimsFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public IServiceProvider _serviceProvider { get; set; }

        public BackendAccountClaimsFactory(IServiceProvider serviceProvider, IAccessTokenProviderAccessor accessor) :
            base(accessor)
        {
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            var userAccount = await base.CreateUserAsync(account, options);

            if (!(userAccount.Identity?.IsAuthenticated ?? false))
            {
                return userAccount;
            }

            if (userAccount.Identity is ClaimsIdentity identity)
            {
                var usersService = _serviceProvider.GetRequiredService<IUserService>();
                try
                {
                    var userRecord = await usersService.GetUserSelf();
                    foreach (var role in userRecord.Roles)
                    {
                        identity.AddClaim(new Claim(identity.RoleClaimType, role));
                    }
                }
                catch (AccessTokenNotAvailableException)
                {
                    // No token, no user record
                    // AccessTokenNotAvailableException is ignored here because it will be handled by the TokenAuthorizationMessageHandler
                }
            }

            return userAccount;
        }
    }
}