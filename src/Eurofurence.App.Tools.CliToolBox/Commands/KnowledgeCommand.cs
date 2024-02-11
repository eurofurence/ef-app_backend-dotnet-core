﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Common.Compression;
using Microsoft.Extensions.CommandLineUtils;
using Eurofurence.App.Domain.Model.Knowledge;
using System.Collections.Generic;
using Eurofurence.App.Domain.Model.Images;

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
            command.Command("export", exportCommand);
            command.Command("import", importCommand);
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

        private void exportCommand(CommandLineApplication command)
        {
            var outputPathArgument = command.Argument("Output path/file", "", false);

            command.OnExecute(() =>
            {
                var archive = ZipFile.Open(outputPathArgument.Value, ZipArchiveMode.Create);

                var knowledgeGroups = _knowledgeGroupService.FindAllAsync().Result;
                var knowledgeEntries = _knowledgeEntryService.FindAllAsync().Result;

                var imageIds = knowledgeEntries.SelectMany(_ => _.ImageIds).ToList();
                var images = _imageService.FindAllAsync(image => imageIds.Contains(image.Id)).Result;

                archive.AddAsJson("knowledgeGroups", knowledgeGroups);
                archive.AddAsJson("knowledgeEntries", knowledgeEntries);
                archive.AddAsJson("images", images);

                foreach(var image in images)
                {
                    archive.AddAsBinary($"imageContent-{image.Id}", _imageService.GetImageContentByIdAsync(image.Id).Result);
                }

                archive.Dispose();
                return 0;
            });

        }

        private void importCommand(CommandLineApplication command)
        {
            var inputPathArgument = command.Argument("Input path/file", "", false);

            command.OnExecute(() =>
            {
                var archive = ZipFile.Open(inputPathArgument.Value, ZipArchiveMode.Read);

                var knowledgeGroups = archive.ReadAsJson<IEnumerable<KnowledgeGroupRecord>>("knowledgeGroups");
                var knowledgeEntries = archive.ReadAsJson<IEnumerable<KnowledgeEntryRecord>>("knowledgeEntries");
                var images = archive.ReadAsJson<IEnumerable<ImageRecord>>("images");

                foreach (var entity in knowledgeGroups) _knowledgeGroupService.InsertOneAsync(entity).Wait();
                foreach (var entity in knowledgeEntries) _knowledgeEntryService.InsertOneAsync(entity).Wait();
                foreach (var entity in images)
                {
                    var imageData = archive.ReadAsBinary($"imageContent-{entity.Id}");
                    _imageService.InsertImageAsync(entity, imageData).Wait();
                }

                archive.Dispose();
                return 0;
            });

        }
    }
}