using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Telegram;
using Eurofurence.App.Server.Services.Abstractions.Telegram;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class UserManager : IUserManager
    {
        private readonly IEntityRepository<UserRecord> _userRepository;

        public UserManager(IEntityRepository<UserRecord> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task GetAclForUserAsync(string username)
        {
            var user = await _userRepository.FindOneAsync(a => a.Username.ToLower() == username.ToLower());
        }

        public async Task SetAclForUserAsync(string username, string acl)
        {
            var user = await _userRepository.FindOneAsync(a => a.Username.ToLower() == username.ToLower());
            if (user == null)
            {
                user = new UserRecord() {Username = username};
                user.NewId();
                await _userRepository.InsertOneAsync(user);
            }

            user.Acl = acl;
            user.Touch();

            await _userRepository.ReplaceOneAsync(user);
        }
    }
}
