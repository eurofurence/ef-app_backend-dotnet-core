using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public interface IRegSysAuthenticationBridge
    {
        Task<bool> VerifyCredentialSetAsync(int regNo, string username, string password);
    }
}