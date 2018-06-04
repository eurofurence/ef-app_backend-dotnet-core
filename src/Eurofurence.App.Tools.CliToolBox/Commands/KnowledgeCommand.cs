using System;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Microsoft.Extensions.CommandLineUtils;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class KnowledgeCommand : ICommand
    {
        private readonly IKnowledgeEntryService _knowledgeEntryService;
        private readonly IKnowledgeGroupService _knowledgeGroupService;
        private readonly IImageService _imageService;

        public KnowledgeCommand(
            IKnowledgeEntryService knowledgeEntryService, 
            IKnowledgeGroupService knowledgeGroupService,
            IImageService imageService)
        {
            _knowledgeEntryService = knowledgeEntryService;
            _knowledgeGroupService = knowledgeGroupService;
            _imageService = imageService;
        }
        
        public string Name => "knowledge";

        public void Register(CommandLineApplication command)
        {
            command.Command("importWikiFile", importWikiFileCommand);
            command.Command("clear", clearCommand);
            command.Command("resetStorageDelta", resetStorageDeltaCommand);
        }

        private void resetStorageDeltaCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                _knowledgeGroupService.ResetStorageDeltaAsync().Wait();
                _knowledgeEntryService.ResetStorageDeltaAsync().Wait();
                return 0;
            });

        }
        private void clearCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                _knowledgeGroupService.DeleteAllAsync().Wait();
                _knowledgeEntryService.DeleteAllAsync().Wait();

                return 0;
            });
        }

        private void importWikiFileCommand(CommandLineApplication command)
        {
            var inputPathOption = command.Option("-inputPath", "Csv file to import", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new Importers.Knowledge.WikiFileImporter(
                    _knowledgeGroupService,
                    _knowledgeEntryService,
                    _imageService
                    );

                var modifiedRecords = importer.ImportWikiFileAsync(inputPathOption.Value()).Result;

                Console.WriteLine(modifiedRecords);

                return modifiedRecords == 0 ? 0 : 1;
            });
        }
    }
}