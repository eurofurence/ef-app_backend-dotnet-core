using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IWnsChannelManager
    {
        Task RegisterChannelAsync(Guid deviceId, string channelUri, string uid, string[] topics);
    }
}