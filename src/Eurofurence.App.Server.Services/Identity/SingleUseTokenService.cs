using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Eurofurence.App.Domain.Model.Identity;
using Eurofurence.App.Server.Services.Abstractions.Identity;

namespace Eurofurence.App.Server.Services.Identity
{
    public class SingleUseTokenService : ISingleUseTokenService
    {
        public const int TokenLength = 128;
        public const string ScopeSeparator = "-";
        ConcurrentDictionary<string, SingleUseTokenPayload> _tokens;

        public SingleUseTokenService()
        {
            _tokens = new();
        }
        public string CreateToken(string scope, SingleUseTokenPayload payload)
        {
            string token;

            do
            {
                token = $"{scope}{ScopeSeparator}{RandomNumberGenerator.GetHexString(TokenLength, true)}";
            } while (!_tokens.TryAdd(token, payload));

            return token;
        }

        public SingleUseTokenPayload TakeTokenPayload(string token)
        {
            SingleUseTokenPayload payload;
            _tokens.Remove(token, out payload);
            if (payload is not null && payload.ValidUntil.CompareTo(DateTimeOffset.UtcNow) > 0)
            {
                return payload;
            }
            return null;
        }

        public bool ValidateScope(string scope, string token)
        {
            return token is not null && scope is not null && token.StartsWith(scope)
                && token.Length == TokenLength + scope.Length + ScopeSeparator.Length;
        }

        public void PruneExpiredTokens()
        {
            var now = DateTimeOffset.UtcNow;
            var expiredTokens = _tokens.Where(t => now.CompareTo(t.Value.ValidUntil) > 0);
            foreach (var expiredToken in expiredTokens)
            {
                _tokens.TryRemove(expiredToken);
            }
        }
    }
}