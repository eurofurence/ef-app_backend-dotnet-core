using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IFcmChannelManager
    {
        Task RegisterDeviceAsync(string deviceId, string uid, string[] topics);
    }
}