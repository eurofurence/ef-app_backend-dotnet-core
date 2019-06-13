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
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
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
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Eurofurence.App.Server.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private ConventionSettings _conventionSettings;
        private ILogger _logger;

        public Startup(IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var client = new MongoClient(new MongoUrl(Configuration["mongoDb:url"]));
            var database = client.GetDatabase(Configuration["mongoDb:database"]);

            _conventionSettings = new ConventionSettings()
            {
                ConventionIdentifier = Configuration["global:conventionIdentifier"],
                IsRegSysAuthenticationEnabled = Convert.ToInt32(Configuration["global:regSysAuthenticationEnabled"]) == 1,
                ApiBaseUrl = Configuration["global:apiBaseUrl"]
            };

            BsonClassMapping.Register();
            CidRouteBaseAttribute.Value = _conventionSettings.ConventionIdentifier;

            services.AddLogging(options =>
            {
                options.ClearProviders();
                
                if (_hostingEnvironment.IsDevelopment())
                {
                    options.AddConsole();
                }
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    cpb => cpb.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddMvc(options =>
            {
                options.MaxModelValidationErrors = 0;
                options.Filters.Add(new CustomValidationAttributesFilter());
            })
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
                options.SwaggerDoc("api", new Info
                {
                    Version = _conventionSettings.ConventionIdentifier,
                    Title = "Eurofurence API for Mobile Apps",
                    Description = "",
                    Contact = new Contact {
                        Name = "Eurofurence IT Department",
                        Email = "it@eurofurence.org",
                        Url = "https://help.eurofurence.org/contact/it"
                    }
                });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Server.Web.xml");
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Domain.Model.xml");


                options.SchemaFilter<IgnoreVirtualPropertiesSchemaFilter>();
                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                options.OperationFilter<BinaryPayloadFilter>();
                options.DocumentFilter<BasePathFilter>(
                    Environment.GetEnvironmentVariable("CID_IN_API_BASE_PATH") == "1"
                    ? $"/{_conventionSettings.ConventionIdentifier}" : "/");
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

            services.Configure<LoggerFilterOptions>(options =>
            {
                options.MinLevel = LogLevel.Trace;
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
            builder.RegisterInstance(_conventionSettings);
            builder.RegisterInstance(new WnsConfiguration
            {
                ClientId = Configuration["wns:clientId"],
                ClientSecret = Configuration["wns:clientSecret"],
                TargetTopic = Configuration["wns:targetTopic"]
            });
            builder.RegisterInstance(new FirebaseConfiguration
            {
                AuthorizationKey = Configuration["firebase:authorizationKey"],
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
            builder.RegisterInstance(new ArtistAlleyConfiguration()
            {
                TelegramAdminGroupChatId = Configuration["artistAlley:telegram:adminGroupChatId"],
                TelegramAnnouncementChannelId = Configuration["artistAlley:telegram:announcementChannelId"],
                TwitterConsumerKey = Configuration["artistAlley:twitter:consumerKey"],
                TwitterConsumerSecret = Configuration["artistAlley:twitter:consumerSecret"],
                TwitterAccessToken = Configuration["artistAlley:twitter:accessToken"],
                TwitterAccessTokenSecret = Configuration["artistAlley:twitter:accessTokenSecret"]
            });

            builder.Register(c => new ApiPrincipal(c.Resolve<IHttpContextAccessor>().HttpContext.User))
                .As<IApiPrincipal>();

            builder.Register(c => Configuration.GetSection("jobs:updateNews"))
                .Keyed<IConfiguration>("updateNews").As<IConfiguration>();
            
            builder.RegisterType<UpdateNewsJob>().WithAttributeFiltering().AsSelf();
            builder.RegisterType<FlushPrivateMessageNotificationsJob>().AsSelf();

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime
        )
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

                var logGroupName = Configuration["aws:cloudwatch:logGroupName"] + "/" + Configuration["global:conventionIdentifier"];

                AWSCredentials credentials =
                    new BasicAWSCredentials(Configuration["aws:accessKey"], Configuration["aws:secret"]);

                IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.EUCentral1);
                var options = new CloudWatchSinkOptions
                {
                    LogGroupName = logGroupName,
                    TextFormatter = new JsonFormatter(),
                    MinimumLogEventLevel = (LogEventLevel)Convert.ToInt32(Configuration["logLevel"]),
                    LogStreamNameProvider = new ConstantLogStreamNameProvider(Environment.MachineName)
                };

                loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
            }

            loggerConfiguration
                .WriteTo
                .Logger(lc =>
                    lc.Filter
                        .ByIncludingOnly($"EventId.Id = {LogEvents.Audit.Id}")
                        .WriteTo.File(Configuration["auditLog"], 
                            restrictedToMinimumLevel: LogEventLevel.Verbose,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{IPAddress}] [{Level}] {Message}{NewLine}{Exception}"
                        )
                );

            var cgc = app.ApplicationServices.GetService<CollectionGameConfiguration>();
            loggerConfiguration
                .WriteTo
                .Logger(lc =>
                    lc.Filter
                        .ByIncludingOnly($"EventId.Id = {LogEvents.CollectionGame.Id}")
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

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("IPAddress",
                    context.Request.Headers.ContainsKey("X-Forwarded-For") ?
                        context.Request.Headers["X-Forwarded-For"].ToString()
                        : context.Connection.RemoteIpAddress.ToString()))
                {
                    await next();
                }
            });

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = $"swagger/ui";
                c.DocExpansion(DocExpansion.None);
                c.SwaggerEndpoint($"../api/swagger.json", "Current API");
                c.InjectStylesheet("/css/swagger.css");
                c.EnableDeepLinking();
            });

            if (env.IsProduction())
            {
                _logger.LogDebug("Starting JobManager to run jobs");
                JobManager.JobFactory = new ServiceProviderJobFactory(app.ApplicationServices);
                JobManager.Initialize(new JobRegistry(Configuration.GetSection("jobs")));
            }

            _logger.LogInformation($"Startup complete ({env.EnvironmentName})");
        }
    }
}