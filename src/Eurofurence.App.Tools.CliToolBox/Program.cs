using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace Eurofurence.App.Tools.CliToolBox
{
    class Program
    {
        static IConfiguration Configuration;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var app = new CommandLineApplication();

            app.Name = "toolbox";
            app.HelpOption("-?|-h|--help");

            app.Command("createToken", (command) =>
            {
                command.Description = "Create a JWT bearer token for authentication against the API";
                command.HelpOption("-?|-h|--help");
                command.ShowInHelpText = true;

                var userArgument = command.Argument("[user]", "Name of the user");
                var rolesArgument = command.Option("-addRole", "Role to add", CommandOptionType.MultipleValue);
                var hoursArgument = command.Option("-validHours", "How many hours the token should be valid (from now on)", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    int hours = 1
                    int.TryParse(hoursArgument.Value(), out hours);

                    Console.WriteLine($"User: {userArgument.Value}");
                    Console.WriteLine($"Roles: {String.Join(",", rolesArgument.Values)}");
                    Console.WriteLine($"Hours: {hours}");

                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, userArgument.Value));
                    foreach (var role in rolesArgument.Values) claims.Add(new Claim(ClaimTypes.Role, role.ToUpper()));

                    var token = CreateOAuthBearerToken(claims, DateTime.UtcNow.AddHours(hours));

                    Console.WriteLine($"\nToken:\n{token}");

                    return 0;
                });
            });

            app.Execute(args);
        }

        static  string CreateOAuthBearerToken(IEnumerable<Claim> claims, DateTime expiration)
        {
            var identity = new ClaimsIdentity(claims);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["oAuth:secretKey"]));

            var descriptor = new SecurityTokenDescriptor()
            {
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Audience = Configuration["oAuth:Audience"],
                Issuer = Configuration["oAuth:Issuer"],
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