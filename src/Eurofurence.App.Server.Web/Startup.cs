using System;
using System.IO;
using System.Text;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using Eurofurence.App.Domain.Model.MongoDb;
using Eurofurence.App.Domain.Model.MongoDb.DependencyResolution;
using Eurofurence.App.Server.Services.Abstraction.Telegram;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Fursuits;
using Eurofurence.App.Server.Services.Security;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Jobs;
using Eurofurence.App.Server.Web.Swagger;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;
using Swashbuckle.AspNetCore.Swagger;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Eurofurence.App.Server.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private ILogger _logger;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var client = new MongoClient(new MongoUrl(Configuration["mongoDb:url"]));
            var database = client.GetDatabase(Configuration["mongoDb:database"]);

            BsonClassMapping.Register();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    cpb => cpb.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddMvc(options => options.MaxModelValidationErrors = 0)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new BaseFirstContractResolver();
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"
                    });
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new Info
                {
                    Version = "v2",
                    Title = "Eurofurence API for Mobile Apps",
                    Description = "",
                    TermsOfService = "None",
                    Contact = new Contact {Name = "Luchs", Url = "https://telegram.me/pinselohrkater"}
                });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/Eurofurence.App.Server.Web.xml");
                options.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/Eurofurence.App.Domain.Model.xml");


                options.SchemaFilter<IgnoreVirtualPropertiesSchemaFilter>();
                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                options.OperationFilter<BinaryPayloadFilter>();
            });


            var oAuthBearerAuthenticationPolicy =
                new AuthorizationPolicyBuilder().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("OAuth-AllAuthenticated", oAuthBearerAuthenticationPolicy);
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(configure =>
                {
                    configure.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(Configuration["oAuth:secretKey"])),
                            ValidAudience = Configuration["oAuth:Audience"],
                            ValidIssuer = Configuration["oAuth:Issuer"],
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromSeconds(0)
                       };
                });

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new AutofacModule(database));
            builder.RegisterModule(new Services.DependencyResolution.AutofacModule());
            builder.RegisterInstance(new TokenFactorySettings
            {
                SecretKey = Configuration["oAuth:secretKey"],
                Audience = Configuration["oAuth:audience"],
                Issuer = Configuration["oAuth:issuer"]
            });
            builder.RegisterInstance(new AuthenticationSettings
            {
                DefaultTokenLifeTime = TimeSpan.FromDays(30)
            });
            builder.RegisterInstance(new ConventionSettings()
            {
                ConventionNumber = 24,
            });
            builder.RegisterInstance(new WnsConfiguration
            {
                ClientId = Configuration["wns:clientId"],
                ClientSecret = Configuration["wns:clientSecret"],
                TargetTopic = Configuration["wns:targetTopic"]
            });
            builder.RegisterInstance(new FirebaseConfiguration
            {
                AuthorizationKey = Configuration["firebase:authorizationKey"],
                TargetTopicAll = Configuration["firebase:targetTopic:all"],
                TargetTopicAndroid = Configuration["firebase:targetTopic:android"],
                TargetTopicIos = Configuration["firebase:targetTopic:ios"]
            });
            builder.RegisterInstance(new TelegramConfiguration
            {
                AccessToken = Configuration["telegram:accessToken"],
                Proxy = Configuration["telegram:proxy"]
            });
            builder.RegisterInstance(new CollectionGameConfiguration()
            {
                LogFile = Configuration["collectionGame:logFile"],
                LogLevel = Convert.ToInt32(Configuration["collectionGame:logLevel"]),
                TelegramManagementChatId = Configuration["collectionGame:telegramManagementChatId"]
            });

            builder.Register(c => new ApiPrincipal(c.Resolve<IHttpContextAccessor>().HttpContext.User))
                .As<IApiPrincipal>();

            builder.Register(c => Configuration.GetSection("jobs:updateNews"))
                .Keyed<IConfiguration>("updateNews").As<IConfiguration>();
            
            builder.RegisterType<UpdateNewsJob>().WithAttributeFiltering().AsSelf();

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

            if (env.IsDevelopment())
            {
                loggerConfiguration
                    .MinimumLevel.Debug()
                    .WriteTo.ColoredConsole();
            }
            else
            {
                loggerConfiguration.MinimumLevel.Is(LogEventLevel.Verbose);

                var logGroupName = Configuration["aws:cloudwatch:logGroupName"] + "/" + env.EnvironmentName;

                AWSCredentials credentials =
                    new BasicAWSCredentials(Configuration["aws:accessKey"], Configuration["aws:secret"]);
                IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.EUCentral1);
                var options = new CloudWatchSinkOptions
                {
                    LogGroupName = logGroupName,
                    LogEventRenderer =  new JsonLogEventRenderer(),
                    MinimumLogEventLevel = (LogEventLevel)Convert.ToInt32(Configuration["logLevel"]),
                    LogStreamNameProvider = new ConstantLogStreamNameProvider(Environment.MachineName)
                };

                loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
            }

            var cgc = serviceProvider.GetService<CollectionGameConfiguration>();
            loggerConfiguration
                .WriteTo
                .Logger(lc =>
                    lc.Filter
                        .ByIncludingOnly(a =>
                            a.Properties.ContainsKey("SourceContext") &&
                            a.Properties["SourceContext"].ToString() == $@"""{typeof(CollectingGameService)}""")
                        .WriteTo.File(cgc.LogFile, (LogEventLevel) cgc.LogLevel)
                );
                
            Log.Logger = loggerConfiguration.CreateLogger();
            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    {"Microsoft", env.IsDevelopment() ? LogLevel.Information : LogLevel.Warning},
                    {"System", env.IsDevelopment() ? LogLevel.Information : LogLevel.Warning}
                })
                .AddSerilog();
            
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.LogInformation($"Logging commences");


            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseCors("CorsPolicy");
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Test}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger/v2/ui";
                c.DocExpansion("none");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
            });

            if (env.IsProduction())
            {
                _logger.LogDebug("Starting JobManager to run jobs");
                JobManager.JobFactory = new ServiceProviderJobFactory(serviceProvider);
                JobManager.Initialize(new JobRegistry(Configuration.GetSection("jobs")));
            }

            _logger.LogInformation($"Startup complete ({env.EnvironmentName})");
        }
    }
}