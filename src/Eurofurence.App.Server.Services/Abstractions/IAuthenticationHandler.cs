using Eurofurence.App.Server.Services.Security;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IAuthenticationHandler
    {
        Task<AuthenticationResponse> AuthorizeViaRegSys(RegSysAuthenticationRequest request);
    }
}