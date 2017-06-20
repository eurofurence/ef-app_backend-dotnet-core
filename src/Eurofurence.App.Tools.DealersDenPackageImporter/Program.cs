using Autofac;
using Eurofurence.App.Domain.Model.MongoDb;
using Eurofurence.App.Domain.Model.MongoDb.DependencyResolution;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using MongoDB.Driver;

namespace Eurofurence.App.Tools.DealersDenPackageImporter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var _client = new MongoClient("mongodb://127.0.0.1:27017");
            var _database = _client.GetDatabase("app_dev");

            BsonClassMapping.Register();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule(_database));
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule());

            var container = builder.Build();

            var importer = new Importer(container.Resolve<IImageService>(), container.Resolve<IDealerService>());

            importer.ImportZipPackageAsync(@"c:\temp\AppData_2016-08-02-amended-v2.zip").Wait();
        }
    }
}