using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;

namespace Eurofurence.App.Tools.CliToolBox.Importers.EventSchedule
{
    public class ImageImporter
    {
        private readonly IImageService _imageService;
        private readonly IEventService _eventService;

        public enum PurposeEnum
        {
            Banner,
            Poster
        }

        public ImageImporter(IImageService imageService, IEventService eventService)
        {
            _imageService = imageService;
            _eventService = eventService;
        }

        public async Task ImportImageAsync(Guid eventId, string imagePath, PurposeEnum imagePurpose)
        {
            var @event = await _eventService.FindOneAsync(eventId);

            if (@event == null) throw new ArgumentException("Event does not exist", nameof(eventId));
            if (!File.Exists(imagePath)) throw new ArgumentException("File does not exist", nameof(imagePath));

            var imageTag = $"event:{imagePurpose}:{eventId}";

            var imageContent = File.ReadAllBytes(imagePath);
            var imageId = await _imageService.InsertOrUpdateImageAsync(imageTag, imageContent);

            switch (imagePurpose)
            {
                    case PurposeEnum.Banner:
                        @event.BannerImageId = imageId;
                        break;
                    case PurposeEnum.Poster:
                        @event.PosterImageId = imageId;
                        break;
            }
            @event.Touch();

            await _eventService.ReplaceOneAsync(@event);

            await Task.Delay(0);
        }
    }
}
