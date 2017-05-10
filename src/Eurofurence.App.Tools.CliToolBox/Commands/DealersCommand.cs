using System;
using Microsoft.Extensions.CommandLineUtils;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class DealersCommand : ICommand
    {
        public string Name => "dealers";

        readonly IDealerService _dealerService;
        readonly IImageService _imageService;

        public DealersCommand(IDealerService dealerService, IImageService imageService)
        {
            _dealerService = dealerService;
            _imageService = imageService;
        }

        public void Register(CommandLineApplication command)
        {
            command.Command("importZipPackage", importZipPackageCommand);
            command.Command("clear", clearCommand);
        }

        private void clearCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                var dealers = _dealerService.FindAllAsync().Result;


                Action<Guid?> dropImage = (imageId) =>
                {
                    if (imageId.HasValue)
                    {
                        Console.WriteLine($"Deleting image {imageId}");
                        _imageService.DeleteOneAsync(imageId.Value).Wait();
                    }
                };

                foreach(var dealer in dealers)
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

        void importZipPackageCommand(CommandLineApplication command)
        {
            var inputPathOption = command.Option("-inputPath", "Zip file to import", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new Importers.DealersDen.PackageImporter(_imageService, _dealerService, command.Out);
                importer.ImportZipPackageAsync(inputPathOption.Value()).Wait();
                return 0;
            });
        }
    }
}
