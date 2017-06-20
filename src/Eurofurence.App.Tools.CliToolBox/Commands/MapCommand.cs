using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;
using Eurofurence.App.Server.Services.Abstractions;
using System.IO;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Maps;

namespace Eurofurence.App.Tools.CliToolBox.Commands
{
    public class MapCommand : ICommand
    {
        public string Name => "map";

        readonly IMapService _mapService;
        readonly IImageService _imageService;

        public MapCommand(IMapService mapService, IImageService imageService)
        {
            _mapService = mapService;
            _imageService = imageService;
        }

        public void Register(CommandLineApplication command)
        {
            command.Command("loadImage", loadImageCommand);
            command.Command("list", listCommand);
        }

        private void listCommand(CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                foreach(var map in _mapService.FindAllAsync().Result)
                {
                    Console.WriteLine($"{map.Id} {map.Description}");
                }
                return 0;
            });


        }

        private void loadImageCommand(CommandLineApplication command)
        {
            var idOption = command.Option("-id", "Guid of the map entry", CommandOptionType.SingleValue);
            var imagePathOption = command.Option("-imagePath", "Path to the image file to load", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var map = _mapService.FindOneAsync(Guid.Parse(idOption.Value())).Result;
                var image = File.Open(imagePathOption.Value(), FileMode.Open, FileAccess.Read);

                var buffer = new byte[image.Length];
                image.Read(buffer, 0, (int)image.Length);

                Console.WriteLine($"Updating map image {map.Id} ({map.Description}) from {imagePathOption.Value()}...");

                var imageId = _imageService.InsertOrUpdateImageAsync($"map:{map.Id}", buffer).Result;
                var imageInfo = _imageService.FindOneAsync(imageId).Result;
                Console.WriteLine($"Image record {imageId} has hash={imageInfo.ContentHashSha1}, last changed at {imageInfo.LastChangeDateTimeUtc} UTC");

                if (map.ImageId != imageId)
                {
                    map.ImageId = imageId;
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
    }
}
