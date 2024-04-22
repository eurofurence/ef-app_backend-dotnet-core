using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Telegram;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface IUserManager
    {
        Task<TEnum> GetAclForUserAsync<TEnum>(string username) where TEnum: struct;

        Task SetAclForUserAsync<TEnum>(string username, TEnum acl);

        IQueryable<UserRecord> GetUsers();
    }
}