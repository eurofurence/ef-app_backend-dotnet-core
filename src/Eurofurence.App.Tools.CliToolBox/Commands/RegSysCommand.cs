using System;
using Eurofurence.App.Tools.CliToolBox.Importers.DealersDen;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class RegSysCommand: ICommand
    {
        private readonly IAuthenticationHandler _authenticationHandler;

        public RegSysCommand(IAuthenticationHandler authenticationHandler)
        {
            _authenticationHandler = authenticationHandler;
        }

        public string Name => "regsys";

        public void Register(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");
            command.Command("createAccessToken", createAccessTokenCommand);
        }

        private void createAccessTokenCommand(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");
            command.ShowInHelpText = true;

            var rolesArgument = command.Option("-grantRole", "Role to grant", CommandOptionType.MultipleValue);

            command.OnExecute(() =>
            {
                command.Out.WriteLine($"Roles: {string.Join(",", rolesArgument.Values)}");

                var token = _authenticationHandler.CreateRegSysAccessTokenAsync(rolesArgument.Values.ToArray()).Result;

                command.Out.WriteLine($"Access Token: {token}");

                return 0;
            });
        }
    }
}