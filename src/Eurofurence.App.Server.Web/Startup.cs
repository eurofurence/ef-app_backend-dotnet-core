using System;
using System.IO;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();

                options.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/Eurofurence.App.Server.Web.xml");
            });


            var oAuthBearerAuthenticationPolicy =
                new AuthorizationPolicyBuilder().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

            services.AddAuthorization(auth => {
                auth.AddPolicy("OAuth-AllAuthenticated", oAuthBearerAuthenticationPolicy); });

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

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {  
                TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(Configuration["oAuth:secretKey"])),
                    ValidAudience = Configuration["oAuth:Audience"],
                    ValidIssuer = Configuration["oAuth:Issuer"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(0)
                }, 
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

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
