using System;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Tools.CliToolBox.Importers.DealersDen;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class ImagesCommand : ICommand
    {
        private readonly IImageService _imageService;

        public ImagesCommand(IImageService imageService)
        {
            _imageService = imageService;
        }

        public string Name => "images";

        public void Register(CommandLineApplication command)
        {
            command.Command("createReference", createReferenceCommand);
            command.Command("deleteReference", deleteReferenceCommand);
            command.Command("resetStorageDelta", resetStorageDeltaCommand);
        }

        private void resetStorageDeltaCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                _imageService.ResetStorageDeltaAsync().Wait();
                return 0;
            });
            
        }

        private void createReferenceCommand(CommandLineApplication command)
        {
            var internalReference = command.Argument("Internal Reference", "", false);

            command.OnExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(internalReference.Value))
                {
                    command.Out.WriteLine("Internal reference not specified");
                    return -1;
                }

                var existingImage = _imageService.FindAll(image => image.InternalReference == internalReference.Value).SingleOrDefault();
                if (existingImage != null)
                {
                    command.Out.WriteLine($"An image with reference {internalReference.Value} already exists.");
                    return -1;
                }

                var image = _imageService.InsertOrUpdateImageAsync(internalReference.Value, _imageService.GeneratePlaceholderImage()).Result;
                command.Out.WriteLine($"Created placeholder image {image.Id} for reference {internalReference.Value}");

                return 0;
            });
        }

        private void deleteReferenceCommand(CommandLineApplication command)
        {
            var internalReference = command.Argument("Internal Reference", "", false);

            command.OnExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(internalReference.Value))
                {
                    command.Out.WriteLine("Internal reference not specified");
                    return -1;
                }

                var existingImage = _imageService.FindAll(image => image.InternalReference == internalReference.Value).SingleOrDefault();
                if (existingImage == null)
                {
                    command.Out.WriteLine($"An image with reference {internalReference.Value} could not be found.");
                    return -1;
                }

                _imageService.DeleteOneAsync(existingImage.Id).Wait();
                
                command.Out.WriteLine($"Deleted image {existingImage.Id} for reference {internalReference.Value}");

                return 0;
            });
        }
    }

}