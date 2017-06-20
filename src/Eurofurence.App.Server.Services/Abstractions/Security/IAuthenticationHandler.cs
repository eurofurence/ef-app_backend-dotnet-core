using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public interface IAuthenticationHandler
    {
        Task<AuthenticationResponse> AuthorizeViaRegSys(RegSysAuthenticationRequest request);
    }
}