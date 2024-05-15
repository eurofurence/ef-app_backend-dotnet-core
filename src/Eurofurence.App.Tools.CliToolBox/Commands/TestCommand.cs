using System;
using McMaster.Extensions.CommandLineUtils;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class TestCommand : ICommand
    {
        public string Name => "test";

        public void Register(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                Console.WriteLine("Testing");

                return 0;
            });
        }
    }
}