using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Telegram;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface ITelegramUserManager
    {
        Task<TEnum> GetAclForUserAsync<TEnum>(string username) where TEnum: struct;

        Task SetAclForUserAsync<TEnum>(string username, TEnum acl);

        Task<IList<TelegramUserRecord>> GetUsersAsync();
    }
}