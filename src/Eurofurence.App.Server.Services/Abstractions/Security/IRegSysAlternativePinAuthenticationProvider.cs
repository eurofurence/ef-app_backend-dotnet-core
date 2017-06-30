using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Security;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public interface IRegSysAlternativePinAuthenticationProvider
    {
        Task<RegSysAlternativePinResponse> RequestAlternativePinAsync(RegSysAlternativePinRequest request, string requesterUid);

        Task<RegSysAlternativePinRecord> GetAlternativePinAsync(int regNo);
    }
}