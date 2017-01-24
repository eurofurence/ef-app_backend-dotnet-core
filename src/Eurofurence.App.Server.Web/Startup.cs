using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Extensions.DependencyInjection;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Serilog;
using Swashbuckle.Swagger.Model;

namespace Eurofurence.App.Server.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.ColoredConsole()
              .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var client = new MongoClient(Configuration["mongoDb:url"]);
            var database = client.GetDatabase(Configuration["mongoDb:database"]);

            Domain.Model.MongoDb.BsonClassMapping.Register();

            services.AddMvc();
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Eurofurence API for Mobile Apps",
                    Description = "",
                    TermsOfService = "None"
                });
                options.DescribeAllEnumsAsStrings();
                options.SchemaFilter<IgnoreVirtualPropertiesSchemaFilter>();

                options.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/Eurofurence.App.Server.Web.xml");
            });

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new Domain.Model.MongoDb.DependencyResolution.AutofacModule(database));
            builder.RegisterModule(new Services.DependencyResolution.AutofacModule());

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerfactory,
            IApplicationLifetime appLifetime)
        {
            loggerfactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyOrigin().AllowAnyMethod());
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Test}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
