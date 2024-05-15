﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eurofurence.App.Server.Services.Abstractions.Security;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.IdentityModel.Tokens;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class TokenCommand : ICommand
    {
        private readonly TokenFactorySettings _settings;

        public TokenCommand(TokenFactorySettings settings)
        {
            _settings = settings;
        }

        public string Name => "token";

        public void Register(CommandLineApplication command)
        {
            command.Command("create", createTokenCommand);
        }

        private void createTokenCommand(CommandLineApplication command)
        {
            command.Description = "Create a JWT bearer token for authentication against the API";
            command.ShowInHelpText = true;

            var userArgument = command.Argument("[user]", "Name of the user");
            var rolesArgument = command.Option("-addRole", "Role to add", CommandOptionType.MultipleValue);
            var hoursArgument = command.Option("-validHours", "How many hours the token should be valid (from now on)",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                if (!int.TryParse(hoursArgument.Value(), out int hours)) hours = 1;

                Console.WriteLine($"User: {userArgument.Value}");
                Console.WriteLine($"Roles: {string.Join(",", rolesArgument.Values)}");
                Console.WriteLine($"Hours: {hours}");

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, userArgument.Value));
                foreach (var role in rolesArgument.Values) claims.Add(new Claim(ClaimTypes.Role, role));

                var token = CreateOAuthBearerToken(claims, DateTime.UtcNow.AddHours(hours));

                Console.WriteLine($"\nToken:\n{token}");

                return 0;
            });
        }

        private string CreateOAuthBearerToken(IEnumerable<Claim> claims, DateTime expiration)
        {
            var identity = new ClaimsIdentity(claims);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.SecretKey));

            var descriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Audience = _settings.Audience,
                Issuer = _settings.Issuer,
                Subject = identity,

                NotBefore = DateTime.UtcNow.AddMinutes(-5),
                IssuedAt = DateTime.UtcNow.AddMinutes(-5),
                Expires = expiration
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);

            var token = handler.WriteToken(securityToken);

            return token;
        }
    }
}