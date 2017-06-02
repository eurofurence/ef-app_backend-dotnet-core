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
using Serilog.Sinks.AwsCloudWatch;
using Swashbuckle.Swagger.Model;
using Eurofurence.App.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Web.Extensions;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;

namespace Eurofurence.App.Server.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

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

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    cpb => cpb.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new BaseFirstContractResolver();
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter()
                        { DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK" });
                    });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
             
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v2",
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
            builder.RegisterInstance(new TokenFactorySettings()
            {
                SecretKey = Configuration["oAuth:secretKey"],
                Audience = Configuration["oAuth:audience"],
                Issuer = Configuration["oAuth:issuer"]
            });
            builder.RegisterInstance(new AuthenticationSettings()
            {
                ConventionNumber = 23,
                DefaultTokenLifeTime = TimeSpan.FromDays(30)
            });
            builder.RegisterInstance(new WnsConfiguration()
            {
                ClientId = Configuration["wns:clientId"],
                ClientSecret = Configuration["wns:clientSecret"]
            });

            builder.Register(c => new ApiPrincipal((c.Resolve<IHttpContextAccessor>().HttpContext.User as ClaimsPrincipal)))
                .As<ApiPrincipal>();

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

            if (env.IsDevelopment())
            {
                loggerConfiguration.WriteTo.ColoredConsole();
            }
            else
            {
                var logGroupName = Configuration["aws:cloudwatch:logGroupName"] + "/" + env.EnvironmentName;

                AWSCredentials credentials = new BasicAWSCredentials(Configuration["aws:accessKey"], Configuration["aws:secret"]);
                IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, Amazon.RegionEndpoint.EUCentral1);
                CloudWatchSinkOptions options = new CloudWatchSinkOptions {
                    LogGroupName = logGroupName,
                    LogEventRenderer = new CustomLogEventRenderer()
                };

                loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", env.IsDevelopment() ? LogLevel.Information : LogLevel.Warning },
                    { "System",  env.IsDevelopment() ? LogLevel.Information : LogLevel.Warning }
                })
               .AddSerilog();


            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseCors("CorsPolicy");

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
            app.UseSwaggerUi("swagger/v2/ui", "/swagger/v2/swagger.json");
        }
    }
}
