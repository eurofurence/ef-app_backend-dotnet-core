using System;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Tools.CliToolBox.Importers.DealersDen;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class DealersCommand : ICommand
    {
        private readonly IDealerService _dealerService;
        private readonly IImageService _imageService;

        public DealersCommand(IDealerService dealerService, IImageService imageService)
        {
            _dealerService = dealerService;
            _imageService = imageService;
        }

        public string Name => "dealers";

        public void Register(CommandLineApplication command)
        {
            command.Command("importZipPackage", importZipPackageCommand);
            command.Command("clear", clearCommand);
            command.Command("clean", cleanCommand);
        }

        private void clearCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                var dealers = _dealerService.FindAll();


                Action<Guid?> dropImage = imageId =>
                {
                    if (imageId.HasValue)
                    {
                        Console.WriteLine($"Deleting image {imageId}");
                        _imageService.DeleteOneAsync(imageId.Value).Wait();
                    }
                };

                foreach (var dealer in dealers)
                {
                    Console.WriteLine($"Deleting images for dealer {dealer.Id} ({dealer.RegistrationNumber})");
                    dropImage(dealer.ArtistImageId);
                    dropImage(dealer.ArtistThumbnailImageId);
                    dropImage(dealer.ArtPreviewImageId);
                }

                _dealerService.DeleteAllAsync().Wait();
                Console.WriteLine("Cleared dealer datastore");

                return 0;
            });
        }

        private void importZipPackageCommand(CommandLineApplication command)
        {
            var inputPathOption = command.Option("-inputPath", "Zip file to import", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new PackageImporter(_imageService, _dealerService, command.Out);
                importer.ImportZipPackageAsync(inputPathOption.Value()).Wait();
                return 0;
            });
        }

        private void cleanCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                Console.WriteLine("Cleaning images");

                var images = _imageService
                    .FindAll(image => image.InternalReference.StartsWith("dealer:"));

                var dealers = _dealerService.FindAll();

                var usedImages = dealers
                    .SelectMany(dealer => new Guid?[] { dealer.ArtistImageId, dealer.ArtistThumbnailImageId, dealer.ArtPreviewImageId })
                    .Where(guid => guid.HasValue)
                    .Select(guid => guid.Value);

                var unusedImages = images
                    .Where(image => !usedImages.Contains(image.Id));


                Console.WriteLine($"{images.Count()} images related to dealers\n{usedImages.Count()} images referenced by {dealers.Count()} dealers");
                Console.WriteLine($"Removing {unusedImages.Count()} images");

                foreach(var image in unusedImages)
                {
                    Console.WriteLine($"  Removing {image.Id}");
                    _imageService.DeleteOneAsync(image.Id).Wait();
                }

                return 0;
            });
        }
    }

}