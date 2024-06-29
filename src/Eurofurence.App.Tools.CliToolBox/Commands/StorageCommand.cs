using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class StorageCommand : ICommand
    {
        private readonly IImageService _imageService;
        private readonly IDealerService _dealerService;
        private readonly IKnowledgeEntryService _knowledgeEntryService;
        private readonly IAnnouncementService _announcementService;
        private readonly IMapService _mapService;
        private readonly IEventService _eventService;

        public StorageCommand(
            IImageService imageService,
            IDealerService dealerService,
            IKnowledgeEntryService knowledgeEntryService,
            IAnnouncementService announcementService,
            IMapService mapService,
            IEventService eventService
            )
        {
            _announcementService = announcementService;
            _imageService = imageService;
            _dealerService = dealerService;
            _knowledgeEntryService = knowledgeEntryService;
            _mapService = mapService;
            _eventService = eventService;
        }

        public string Name => "storage";

        public void Register(CommandLineApplication command)
        {
            command.Command("listUnusedImages", listUnusedImagesCommand);
            command.Command("deleteUnusedImages", deleteUnusedImagesCommand);
        }

        private async Task<ImageRecord[]> GetUnusedImagesAsync()
        {
            var images = _imageService.FindAll();

            var usedImageIdSources = new IEnumerable<Guid?>[] {
                ( _dealerService.FindAll()).ToList().SelectMany(a => new Guid?[] { a.ArtistImageId, a.ArtistThumbnailImageId, a.ArtPreviewImageId }),
                ( _eventService.FindAll()).ToList().SelectMany(a => new Guid?[] { a.BannerImageId, a.PosterImageId }),
                ( _announcementService.FindAll()).Select(a => a.ImageId),
                ( _knowledgeEntryService.FindAll()).SelectMany(a => a.Images.Select(b => b.Id as Guid?)),
                ( _mapService.FindAll()).Select(a => a.ImageId as Guid?)
            };

            var usedImageIds = usedImageIdSources
                .SelectMany(a => a)
                .Where(a => a.HasValue)
                .Select(a => a.Value)
                .ToArray();

            return images.Where(a => !usedImageIds.Contains(a.Id)).ToArray();
        }

        private void listUnusedImagesCommand(CommandLineApplication command)
        {
            command.Description = "Shows all unused image ids";

            command.OnExecute(() =>
            {
                var unusedImages = GetUnusedImagesAsync().Result;

                unusedImages.ToList().ForEach(a =>
                    command.Out.WriteLine($"{a.Id}, {a.Width}x{a.Height} px, {a.SizeInBytes} bytes"));

                return 0;
            });
        }

        private void deleteUnusedImagesCommand(CommandLineApplication command)
        {
            command.Description = "Deletes all unused images";

            command.OnExecute(() =>
            {
                var unusedImages = GetUnusedImagesAsync().Result;
                unusedImages.ToList().ForEach(a => _imageService.DeleteOneAsync(a.Id).Wait());

                command.Out.WriteLine($"Deleted {unusedImages.Count()} image(s), {unusedImages.Sum(a => a.SizeInBytes)} bytes in total");

                return 0;
            });
        }
    }
}