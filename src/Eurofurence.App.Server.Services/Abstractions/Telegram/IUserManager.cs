using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface IUserManager
    {
        Task GetAclForUserAsync(string username);

        Task SetAclForUserAsync(string username, string acl);
    }
}