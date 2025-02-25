using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Telegram;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class UserManager : IUserManager
    {
        private readonly AppDbContext _appDbContext;

        public UserManager(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public async Task<TEnum> GetAclForUserAsync<TEnum>(string username) where TEnum : struct
        {
            var record = await _appDbContext.TelegramUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());

            Enum.TryParse(record?.Acl, out TEnum acl);

            return acl;
        }

        public async Task SetAclForUserAsync<TEnum>(string username, TEnum acl)
        {
            var record = await _appDbContext.TelegramUsers.FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());

            if (record == null)
            {
                record = new TelegramUserRecord()
                {
                    Username = username
                };
                record.NewId();
                record.Touch();

                _appDbContext.TelegramUsers.Add(record);
                await _appDbContext.SaveChangesAsync();
                await SetAclForUserAsync(username, acl);

                return;
            }

            record.Acl = acl.ToString();
            record.Touch();

            _appDbContext.TelegramUsers.Update(record);
            await _appDbContext.SaveChangesAsync();
        }

        public IQueryable<TelegramUserRecord> GetUsers()
        {
            return _appDbContext.TelegramUsers;
        }
    }
}
