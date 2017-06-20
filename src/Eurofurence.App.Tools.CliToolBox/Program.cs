using Autofac;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.Security;
using Eurofurence.App.Tools.CliToolBox.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Tools.CliToolBox
{
    class Program
    {
        static IConfiguration Configuration;

        static void Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = configurationBuilder.Build();

            var self = typeof(CliToolBox.Program).GetTypeInfo().Assembly;
            var client = new MongoClient(Configuration["mongoDb:url"]);
            var database = client.GetDatabase(Configuration["mongoDb:database"]);

            Domain.Model.MongoDb.BsonClassMapping.Register();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new Domain.Model.MongoDb.DependencyResolution.AutofacModule(database));
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule());

            builder.RegisterInstance(new TokenFactorySettings()
            {
                SecretKey = Configuration["oAuth:secretKey"],
                Audience = Configuration["oAuth:audience"],
                Issuer = Configuration["oAuth:issuer"]
            });

            var commands = self.GetTypes().Where(a => typeof(ICommand).IsAssignableFrom(a) && !a.GetTypeInfo().IsAbstract);

            foreach (var @type in commands)
                builder.RegisterType(@type).AsSelf();

            var container = builder.Build();

            var app = new CommandLineApplication();


            app.Name = "toolbox";
            app.HelpOption("-?|-h|--help");

            foreach (var @type in commands)
            {
                ICommand command = (ICommand)container.Resolve(@type);
                app.Command(command.Name, command.Register);
            }

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
    }
}