using System;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Tools.CliToolBox.Importers.EventSchedule;
using Microsoft.Extensions.CommandLineUtils;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class EventsCommand : ICommand
    {
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;
        private readonly IImageService _imageService;

        public EventsCommand(
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService,
            IImageService imageService
            )
        {
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;
            _imageService = imageService;
        }

        public string Name => "events";

        public void Register(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");
            command.Command("importCsvFile", importCsvFileCommand);
            command.Command("importImage", importImageCommand);
            command.Command("setTags", setTagsCommand);
            command.Command("clear", clearCommand);
        }

        private void clearCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                _eventService.DeleteAllAsync().Wait();
                _eventConferenceDayService.DeleteAllAsync().Wait();
                _eventConferenceRoomService.DeleteAllAsync().Wait();
                _eventConferenceTrackService.DeleteAllAsync().Wait();

                return 0;
            });
        }

        private void setTagsCommand(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");

            var eventIdOption = command.Option("-eventId", "Event Id", CommandOptionType.SingleValue);
            var tags = command.Option("-tags", "Tags (Comma-Delimited)", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new ImageImporter(_imageService, _eventService);
                ImageImporter.PurposeEnum purpose = ImageImporter.PurposeEnum.Banner;
                Guid eventId = Guid.Empty;

                if (!Guid.TryParse(eventIdOption.Value(), out eventId))
                {
                    command.Out.WriteLine("Invalid value for -purpose or -eventId");
                    return -1;
                }

                var @event = _eventService.FindOneAsync(eventId).Result;

                if (@event == null)
                {
                    command.Out.WriteLine($"Event {eventId} not found.");
                    return -1;
                }

                @event.Tags = tags.Value().Split(',');
                @event.Touch();

                _eventService.ReplaceOneAsync(@event).Wait();

                command.Out.WriteLine($"Event {eventId} ({@event.Title} - {@event.SubTitle}) updated with tags: {String.Join(", ", @event.Tags)}");

                return 0;
            });
        }

        private void importImageCommand(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");

            var eventIdOption = command.Option("-eventId", "Event Id", CommandOptionType.SingleValue);
            var imagePathOption = command.Option("-imagePath", "ImagePath", CommandOptionType.SingleValue);
            var purposeOption = command.Option("-purpose", "Banner or Poster", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new ImageImporter(_imageService, _eventService);
                ImageImporter.PurposeEnum purpose = ImageImporter.PurposeEnum.Banner;
                Guid eventId = Guid.Empty;

                if (!Enum.TryParse(purposeOption.Value(), out purpose) || !Guid.TryParse(eventIdOption.Value(), out eventId))
                {
                    Console.WriteLine("Invalid value for -purpose or -eventId");
                    return -1;
                }

                try
                {
                    importer.ImportImageAsync(eventId, imagePathOption.Value(), purpose).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return -1;
                }
                
                return 0;
            });
        }

        private void importCsvFileCommand(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");

            var inputPathOption = command.Option("-inputPath", "Csv file to import", CommandOptionType.SingleValue);
            var fakeStartDate = command.Option("-fakeStartDate", "Fake start date (first con day) to shift all content to", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                DateTime parsedFakeStartDate;
                bool hasFakeStartDate = DateTime.TryParse(fakeStartDate.Value(), out parsedFakeStartDate);

                var importer = new Importers.EventSchedule.CsvFileImporter(
                    _eventService,
                    _eventConferenceDayService,
                    _eventConferenceRoomService,
                    _eventConferenceTrackService
                    )
                {
                    FakeStartDate = hasFakeStartDate ? (DateTime?)parsedFakeStartDate : null
                };

                var modifiedRecords = importer.ImportCsvFile(inputPathOption.Value());

                Console.WriteLine(modifiedRecords);

                return modifiedRecords == 0 ? 0 : 1;
            });
        }
    }
}