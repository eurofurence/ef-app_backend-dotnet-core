using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IUserService
    {
        public Task<UserRecord> GetUserSelf();

        /// <summary>
        /// Changes the artist alley status of a given user (by <paramref name="userID"/>)
        /// 
        /// This can be: BANNED or OK
        /// </summary>
        /// <param name="userID">The id of the user</param>
        /// <param name="changeRequest"></param>
        /// <returns></returns>
        public Task PutUserArtistAlleyStatusAsync(string userID, ArtistAlleyUserPenaltyChangeRequest changeRequest);
        
    }
}
