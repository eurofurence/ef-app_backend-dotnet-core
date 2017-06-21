using System;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.Extensions.CommandLineUtils;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class EventsCommand : ICommand
    {
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;

        public EventsCommand(
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService
            )
        {
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;
        }

        public string Name => "events";

        public void Register(CommandLineApplication command)
        {
            command.Command("importCsvFile", importCsvFileCommand);
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

        private void importCsvFileCommand(CommandLineApplication command)
        {
            var inputPathOption = command.Option("-inputPath", "Csv file to import", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var importer = new Importers.EventSchedule.CsvFileImporter(
                    _eventService,
                    _eventConferenceDayService,
                    _eventConferenceRoomService,
                    _eventConferenceTrackService
                    );

                var modifiedRecords = importer.ImportCsvFile(inputPathOption.Value());

                Console.WriteLine(modifiedRecords);

                return modifiedRecords == 0 ? 0 : 1;
            });
        }
    }
}