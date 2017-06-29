using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    interface IAuthenticationProvider
    {
        Task<AuthenticationResult> ValidateRegSysAuthenticationRequestAsync(RegSysAuthenticationRequest request);
    }
}
