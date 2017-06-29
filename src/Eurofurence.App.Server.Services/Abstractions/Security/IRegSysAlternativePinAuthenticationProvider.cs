using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public interface IRegSysAlternativePinAuthenticationProvider
    {
        Task<RegSysAlternativePinResponse> RequestAlternativePinAsync(RegSysAlternativePinRequest request, string requesterUid);
    }
}