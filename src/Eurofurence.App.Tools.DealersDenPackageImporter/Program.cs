using Autofac;
using Eurofurence.App.Server.Services.Abstractions;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Eurofurence.App.Tools.DealersDenPackageImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var _client = new MongoClient("mongodb://127.0.0.1:27017");
            var _database = _client.GetDatabase("app_dev");

            Domain.Model.MongoDb.BsonClassMapping.Register();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new Domain.Model.MongoDb.DependencyResolution.AutofacModule(_database));
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule());

            var container = builder.Build();

            var importer = new Importer(container.Resolve<IImageService>(), container.Resolve<IDealerService>());

            importer.ImportZipPackageAsync(@"c:\temp\AppData_2016-08-02-amended-v2.zip").Wait();
        }
    }
}