using System;
using System.IO;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using McMaster.Extensions.CommandLineUtils;
using Eurofurence.App.Domain.Model.Maps;
using System.Collections.Generic;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class MapCommand : ICommand
    {
        private readonly IImageService _imageService;

        private readonly IMapService _mapService;

        public MapCommand(IMapService mapService, IImageService imageService)
        {
            _mapService = mapService;
            _imageService = imageService;
        }

        public string Name => "map";

        public void Register(CommandLineApplication command)
        {
            command.Command("loadImage", loadImageCommand);
            command.Command("list", listCommand);
            command.Command("create", createCommand);
            command.Command("update", updateCommand);
            command.Command("delete", deleteCommand);
            command.Command("resetStorageDelta", resetStorageDeltaCommand);
        }

        private void resetStorageDeltaCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                _mapService.ResetStorageDeltaAsync().Wait();
                return 0;
            });

        }


        private void listCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                foreach (var map in _mapService.FindAll())
                    Console.WriteLine($"{map.Id} {map.Description}");
                return 0;
            });
        }

        private void createCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                var id = Guid.NewGuid();
                var record = new MapRecord()
                {
                    Id = id,
                    Description = id.ToString(),
                    IsBrowseable = false,
                    Entries = new List<MapEntryRecord>()
                };

                _mapService.InsertOneAsync(record).Wait();

                command.Out.WriteLine($"New map id: {id}");

                return 0;
            });
        }

        private void updateCommand(CommandLineApplication command)
        {

            var idOption = command.Option("-id", "Guid of the map entry", CommandOptionType.SingleValue);
            var isBrowseableOption = command.Option("-isBrowseable", "", CommandOptionType.SingleValue);
            var orderOption = command.Option("-order", "", CommandOptionType.SingleValue);
            var descriptionOption = command.Option("-description", "", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {

                if (!idOption.HasValue())
                {
                    command.Out.WriteLine("-id is required");
                    return -1;
                }

                var map = _mapService.FindOneAsync(Guid.Parse(idOption.Value())).Result;

                if (map == null)
                {
                    command.Out.WriteLine($"No map with id {idOption.Value()} found.");
                    return -1;
                }
                command.Out.WriteLine($"Updating map {map.Id} (Description={map.Description}, IsBrowseable={map.IsBrowseable})");

                if (isBrowseableOption.HasValue())
                {
                    map.IsBrowseable = isBrowseableOption.Value() == "1";
                    command.Out.WriteLine($"  Setting IsBrowseable to {map.IsBrowseable}");
                }
                if (descriptionOption.HasValue())
                {
                    map.Description = descriptionOption.Value();
                    command.Out.WriteLine($"  Setting Description to {map.Description}");
                }

                int newOrder = 0;
                if (orderOption.HasValue() && Int32.TryParse(orderOption.Value(), out newOrder))
                {
                    map.Order = newOrder;
                    command.Out.WriteLine($"  Setting Order to {map.Order}");
                }

                _mapService.ReplaceOneAsync(map).Wait();

                return 0;
            });
        }

        private void loadImageCommand(CommandLineApplication command)
        {
            var idOption = command.Option("-id", "Guid of the map entry", CommandOptionType.SingleValue);
            var imagePathOption = command.Option("-imagePath", "Path to the image file to load",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var map = _mapService.FindOneAsync(Guid.Parse(idOption.Value())).Result;
                var image = File.Open(imagePathOption.Value(), FileMode.Open, FileAccess.Read);

                Console.WriteLine($"Updating map image {map.Id} ({map.Description}) from {imagePathOption.Value()}...");

                var imageRecord = _imageService.InsertImageAsync($"map:{map.Id}", image).Result;
                Console.WriteLine(
                    $"Image record {imageRecord.Id} has hash={imageRecord.ContentHashSha1}, last changed at {imageRecord.LastChangeDateTimeUtc} UTC");

                if (map.ImageId != imageRecord.Id)
                {
                    map.ImageId = imageRecord.Id;
                    map.Touch();
                    _mapService.ReplaceOneAsync(map);
                    Console.WriteLine("Map record has been updated.");
                }
                else
                {
                    Console.WriteLine("Map record has not changed.");
                }

                return 0;
            });
        }

        private void deleteCommand(CommandLineApplication command)
        {
            var idOption = command.Option("-id", "Guid of the map entry", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                if (!idOption.HasValue())
                {
                    command.Out.WriteLine("-id is required");
                    return -1;
                }

                var map = _mapService.FindOneAsync(Guid.Parse(idOption.Value())).Result;

                if (map == null)
                {
                    command.Out.WriteLine($"No map with id {idOption.Value()} found.");
                    return -1;
                }
                command.Out.WriteLine($"Deleting map {map.Id} (Description={map.Description}, IsBrowseable={map.IsBrowseable})");

                _mapService.DeleteOneAsync(map.Id).Wait();
                if (map.ImageId != null)
                {
                    _imageService.DeleteOneAsync((Guid)map.ImageId).Wait();
                }

                return 0;
            });
        }
    }
}