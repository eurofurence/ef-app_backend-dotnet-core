using Eurofurence.App.Domain.Model.Identity;

namespace Eurofurence.App.Server.Services.Abstractions.Identity
{
    public interface ISingleUseTokenService
    {
        /// <summary>
        /// Create and return a new token for given scope with provided payload.
        /// </summary>
        /// <param name="scope">scope for the token; will be prefixed to token string</param>
        /// <param name="payload">payload connected to the token</param>
        /// <returns>unique token string</returns>
        public string CreateToken(string scope, SingleUseTokenPayload payload);
        /// <summary>
        /// Retrieve payload connected to given token and immediately invalidate the token.
        /// </summary>
        /// <param name="token">token for which to retrieve payload</param>
        /// <returns>token payload or <c>null</c> if invalid or expired token</returns>
        public SingleUseTokenPayload TakeTokenPayload(string token);
        /// <summary>
        /// Check if the token conforms with given scope.
        /// </summary>
        /// <param name="scope">scope for which to check</param>
        /// <param name="token">token to be validated</param>
        /// <returns><c>true</c> if token is in <c>scope</c></returns>
        public bool ValidateScope(string scope, string token);
        /// <summary>
        /// Remove all expired tokens.
        /// </summary>
        public void PruneExpiredTokens();
    }
}
