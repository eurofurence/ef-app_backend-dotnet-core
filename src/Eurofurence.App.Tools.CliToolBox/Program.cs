using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Eurofurence.App.Domain.Model.MongoDb;
using Eurofurence.App.Domain.Model.MongoDb.DependencyResolution;
using Eurofurence.App.Tools.CliToolBox.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Tools.CliToolBox
{
    internal class Program
    {
        private static IConfiguration Configuration;

        private static int Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = configurationBuilder.Build();

            var self = typeof(Program).GetTypeInfo().Assembly;
            var client = new MongoClient(new MongoUrl(Configuration["mongoDb:url"]));
            var database = client.GetDatabase(Configuration["mongoDb:database"]);

            BsonClassMapping.Register();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule(Configuration));
            builder.Register<ILoggerFactory>(_ => new LoggerFactory());

            var commands = self.GetTypes()
                .Where(a => typeof(ICommand).IsAssignableFrom(a) && !a.GetTypeInfo().IsAbstract);

            foreach (var type in commands)
                builder.RegisterType(type).AsSelf();

            var container = builder.Build();
            container.Resolve<IMongoDatabaseInitialization>().ExecuteInitializationTasks(database);

            var app = new CommandLineApplication();

            app.Name = "toolbox";

            if (args.Length == 0)
                args = new[] {"-h"};
            
            
            foreach (var type in commands)
            {
                var command = (ICommand) container.Resolve(type);
                app.Command(command.Name, command.Register);
            }

            if (args.Length == 1 && args[0] == "--documentation")
            {
                PrintAllHelpOptionsRecursively(app);
                return 0;
            }

            AddHelpOptionRecursively(app);

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        public static void AddHelpOptionRecursively(CommandLineApplication app)
        {
            app.HelpOption("-?|-h|--help");
            foreach(var subCommand in app.Commands)
            {
                AddHelpOptionRecursively(subCommand);
            }
        }

        public static void PrintAllHelpOptionsRecursively(CommandLineApplication app)
        {
            app.ShowHelp();
            foreach (var subCommand in app.Commands)
            {
                PrintAllHelpOptionsRecursively(subCommand);
            }
        }
    }
}