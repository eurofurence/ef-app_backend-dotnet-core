using CsvHelper;
using CsvHelper.Configuration;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Eurofurence.App.Tools.DealersDenPackageImporter
{
    public class Importer
    {
        private IImageService _imageService;

        public Importer(IImageService imageService)
        {
            _imageService = imageService;
        }

        public void ImportZipPackage(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            using (var archive = new ZipArchive(fileStream))
            {
                var csvEntry = archive.Entries.Single(a => a.Name.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase));

                TextReader reader = new StreamReader(csvEntry.Open());

                var csvReader = new CsvReader(reader);
                csvReader.Configuration.RegisterClassMap<DealerImportRowClassMap>();
                var csvRecords = csvReader.GetRecords<DealerImportRow>().ToList();

                foreach(var record in csvRecords)
                {
                    var artistThumbnailImage = archive.Entries.SingleOrDefault(a => a.Name.StartsWith($"artist_{record.RegNo}.", StringComparison.CurrentCultureIgnoreCase));

                    if (artistThumbnailImage != null) {

                        using (var s = artistThumbnailImage.Open())
                        using (var br = new BinaryReader(s))
                        {
                            var imageByteArray = br.ReadBytes((int)artistThumbnailImage.Length);
                            _imageService.InsertOrUpdateImageAsync($"dealer:artistThumbnailImage:{record.RegNo}", imageByteArray).Wait();
                        }
                    }



                }

            }
        }
    }

    public sealed class DealerImportRowClassMap : CsvClassMap<DealerImportRow>
    {
        public DealerImportRowClassMap()
        {
            Map(m => m.RegNo).Name("Reg No.");
            Map(m => m.Nickname).Name("Nick");
            Map(m => m.DisplayName).Name("Display Name");
            Map(m => m.WebsiteUrl).Name("Website");
            Map(m => m.Merchandise).Name("Merchandise");
            Map(m => m.ShortDescription).Name("Short Description");
            Map(m => m.AboutTheArtist).Name("About the Artist");
            Map(m => m.AboutTheArt).Name("About the Art");
            Map(m => m.ArtPreviewCaption).Name("Art Preview Caption");
        }
    }

    public class DealerImportRow
    {
        public int RegNo { get; set; }
        public string Nickname { get; set; }
        public string DisplayName { get; set; }
        public string WebsiteUrl { get; set; }
        public string Merchandise { get; set; }
        public string ShortDescription { get; set; }
        public string AboutTheArtist { get; set; }
        public string AboutTheArt { get; set; }
        public string ArtPreviewCaption { get; set; }
    }
}