using McMaster.Extensions.CommandLineUtils;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public interface ICommand
    {
        string Name { get; }

        void Register(CommandLineApplication command);
    }
}