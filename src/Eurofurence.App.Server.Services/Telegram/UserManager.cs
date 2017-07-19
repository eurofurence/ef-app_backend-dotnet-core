using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Telegram;
using Eurofurence.App.Server.Services.Abstractions.Telegram;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class UserManager : IUserManager
    {
        private readonly IEntityRepository<UserRecord> _telegramUserRepository;

        public UserManager(IEntityRepository<UserRecord> telegramUserRepository)
        {
            _telegramUserRepository = telegramUserRepository;
        }


        public async Task<TEnum> GetAclForUserAsync<TEnum>(string username) where TEnum : struct
        {
            var record = await _telegramUserRepository.FindOneAsync(a => a.Username.ToLower() == username.ToLower());

            TEnum acl = default(TEnum);
            Enum.TryParse(record?.Acl, out acl);

            return acl;
        }

        public async Task SetAclForUserAsync<TEnum>(string username, TEnum acl)
        {
            var record = await _telegramUserRepository.FindOneAsync(a => a.Username.ToLower() == username.ToLower());

            if (record == null)
            {
                record = new UserRecord()
                {
                    Username = username
                };
                record.NewId();
                record.Touch();

                await _telegramUserRepository.InsertOneAsync(record);
                await SetAclForUserAsync(username, acl);

                return;
            }

            record.Acl = acl.ToString();
            record.Touch();

            await _telegramUserRepository.ReplaceOneAsync(record);
        }

        public async Task<IList<UserRecord>> GetUsersAsync()
        {
            return (await _telegramUserRepository.FindAllAsync()).ToList();
        }
    }
}
