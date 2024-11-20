using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Jobs;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Web.Identity;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Quartz;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Telegram;
using Eurofurence.App.Server.Web.Telegram;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using dotAPNS.AspNetCore;
using System.Collections.Generic;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.MinIO;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Services.Announcements;
using Eurofurence.App.Server.Services.ArtistsAlley;
using Eurofurence.App.Server.Services.ArtShow;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Dealers;
using Eurofurence.App.Server.Services.Events;
using Eurofurence.App.Server.Services.Fursuits;
using Eurofurence.App.Server.Services.Images;
using Eurofurence.App.Server.Services.Knowledge;
using Eurofurence.App.Server.Services.Lassie;
using Eurofurence.App.Server.Services.LostAndFound;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Services.Sanitization;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Services.Users;
using Eurofurence.App.Server.Services.Validation;

namespace Eurofurence.App.Server.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private ILogger _logger;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var globalOptions = Configuration.GetSection("Global").Get<GlobalOptions>();
            var lassieOptions = Configuration.GetSection("Lassie").Get<LassieOptions>();
            var dealerOptions = Configuration.GetSection("Dealers").Get<DealerOptions>();
            var announcementOptions = Configuration.GetSection("Announcements").Get<AnnouncementOptions>();
            var eventOptions = Configuration.GetSection("Events").Get<EventOptions>();

            // Configuration from appsettings.json
            services.Configure<GlobalOptions>(Configuration.GetSection("Global"));
            services.Configure<FirebaseOptions>(Configuration.GetSection("Push:Firebase"));
            services.Configure<ApnsOptions>(Configuration.GetSection("Push:Apns"));
            services.Configure<ExpoOptions>(Configuration.GetSection("Push:Expo"));
            services.Configure<QrCodeConfiguration>(Configuration.GetSection("QrCode"));
            services.Configure<ArtistAlleyOptions>(Configuration.GetSection("ArtistAlley"));
            services.Configure<LassieOptions>(Configuration.GetSection("Lassie"));
            services.Configure<MinIoOptions>(Configuration.GetSection("MinIo"));
            services.Configure<DealerOptions>(Configuration.GetSection("Dealers"));
            services.Configure<AnnouncementOptions>(Configuration.GetSection("Announcements"));
            services.Configure<EventOptions>(Configuration.GetSection("Events"));
            services.Configure<CollectionGameOptions>(Configuration.GetSection("CollectionGame"));
            services.Configure<IdentityOptions>(Configuration.GetSection("Identity"));
            services.Configure<AuthorizationOptions>(Configuration.GetSection("Authorization"));
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
            });
            services.ConfigureOptions<ConfigureOAuth2IntrospectionOptions>();

            // Register services
            services.AddTransient<IAgentClosingResultService, AgentClosingResultService>();
            services.AddTransient<IAnnouncementService, AnnouncementService>();
            services.AddTransient<ICollectingGameService, CollectingGameService>();
            services.AddTransient<IDealerService, DealerService>();
            services.AddTransient<IEventConferenceDayService, EventConferenceDayService>();
            services.AddTransient<IEventConferenceRoomService, EventConferenceRoomService>();
            services.AddTransient<IEventConferenceTrackService, EventConferenceTrackService>();
            services.AddTransient<IEventFeedbackService, EventFeedbackService>();
            services.AddTransient<IEventService, EventService>();
            services.AddTransient<IPushNotificationChannelManager, PushNotificationChannelManager>();
            services.AddTransient<IFursuitBadgeService, FursuitBadgeService>();
            services.AddTransient<IHtmlSanitizer, GanssHtmlSanitizer>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IItemActivityService, ItemActivityService>();
            services.AddTransient<IKnowledgeEntryService, KnowledgeEntryService>();
            services.AddTransient<IKnowledgeGroupService, KnowledgeGroupService>();
            services.AddTransient<ILassieApiClient, LassieApiClient>();
            services.AddTransient<ILinkFragmentValidator, LinkFragmentValidator>();
            services.AddTransient<ILostAndFoundService, LostAndFoundService>();
            services.AddTransient<ILostAndFoundLassieImporter, LostAndFoundLassieImporter>();
            services.AddTransient<IMapService, MapService>();
            services.AddTransient<IPrivateMessageService, PrivateMessageService>();
            services.AddTransient<IPushNotificationChannelStatisticsService, PushNotificationChannelStatisticsService>();
            services.AddTransient<IQrCodeService, QrCodeService>();
            services.AddTransient<IStorageServiceFactory, StorageServiceFactory>();
            services.AddTransient<ITableRegistrationService, TableRegistrationService>();
            services.AddTransient<IHttpUriSanitizer, HttpUriSanitizer>();
            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<IDealerApiClient, DealerApiClient>();
            services.AddTransient<IDeviceIdentityService, DeviceIdentityService>();
            services.AddTransient<IRegistrationIdentityService, RegistrationIdentityService>();
            services.AddTransient<IArtistAlleyUserPenaltyService, ArtistAlleyUserPenaltyService>();
            services.AddSingleton<ITelegramMessageBroker, TelegramMessageBroker>();
            services.AddSingleton<IPrivateMessageQueueService, PrivateMessageQueueService>();

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
                        .AllowAnyHeader());
            });

            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.MaxModelValidationErrors = 0;
                    options.Filters.Add(new CustomValidationAttributesFilter());
                })
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opt.JsonSerializerOptions.WriteIndented = true;
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opt.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(), new JsonDateTimeConverter() }
            };

            services.AddSingleton(s => serializerOptions);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("api", new OpenApiInfo
                {
                    Version = globalOptions.ConventionIdentifier,
                    Title = "Eurofurence API for Mobile Apps",
                    Description = "",
                    Contact = new OpenApiContact
                    {
                        Name = "Eurofurence IT Department",
                        Email = "it@eurofurence.org",
                        Url = new Uri("https://help.eurofurence.org/contact/it")
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Eurofurence Identity",
                    Description = "Authenticate with Eurofurence Identity Provider",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.Http
                });

                options.AddSecurityDefinition(ApiKeyAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = ApiKeyAuthenticationDefaults.HeaderName,
                    Description = "Authenticate with a static API key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                //options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Server.Web.xml");
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Domain.Model.xml");

                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                options.OperationFilter<BinaryPayloadFilter>();

                if (Environment.GetEnvironmentVariable("CID_IN_API_BASE_PATH") == "1")
                {
                    options.AddServer(new OpenApiServer()
                    {
                        Description = "nginx (CID in path)",
                        Url = $"/{globalOptions.ConventionIdentifier}"
                    });
                }
                else
                {
                    options.AddServer(new OpenApiServer()
                    {
                        Description = "local",
                        Url = "/"
                    });
                }
            });

            services.AddTransient<IClaimsTransformation, RolesClaimsTransformation>();
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options => { options.EnableCaching = true; })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>
                (ApiKeyAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ApiKeys = Configuration.GetSection("ApiKeys")
                            .Get<IList<ApiKeyAuthenticationOptions.ApiKeyOptions>>();
                        foreach (var apiKey in options.ApiKeys ?? []) {
                            _logger.LogInformation($"Configured API key for {apiKey.PrincipalName} with roles {string.Join(',', apiKey.Roles)} valid until {apiKey.ValidUntil}.");
                        }
                    });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            if (Configuration.GetSection("Telegram").GetSection("AccessToken").Get<string>() is { Length: > 0 })
            {
                services.Configure<TelegramConfiguration>(Configuration.GetSection("Telegram"));
                services.AddHostedService<BotService>();
                services.AddSingleton<ITelegramBotClient>(sp => new TelegramBotClient(
                    sp.GetRequiredService<IOptions<TelegramConfiguration>>().Value.AccessToken
                ));
                services.AddTransient<AdminConversation>();
            }

            services.Configure<LoggerFilterOptions>(options => { options.MinLevel = LogLevel.Trace; });

            services.AddDbContextPool<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("Eurofurence");

                var serverVersionString = Environment.GetEnvironmentVariable("MYSQL_VERSION");
                if (string.IsNullOrEmpty(serverVersionString) || !ServerVersion.TryParse(serverVersionString, out var serverVersion))
                {
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }

                options.UseMySql(
                    connectionString,
                    serverVersion,
                    mySqlOptions => mySqlOptions.UseMicrosoftJson());
            });

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            services.AddApns();

            ILogger logger = loggerFactory.CreateLogger<Startup>();

            services.AddQuartz(q =>
            {
                var flushPrivateMessageNotificationsConfiguration = Configuration
                    .GetSection("jobs:flushPrivateMessageNotifications").Get<JobConfiguration>();
                var updateAnnouncementsConfiguration = Configuration
                    .GetSection("jobs:updateAnnouncements").Get<JobConfiguration>();
                var updateDealersConfiguration = Configuration
                    .GetSection("jobs:updateDealers").Get<JobConfiguration>();
                var updateEventsConfiguration = Configuration
                    .GetSection("jobs:updateEvents").Get<JobConfiguration>();
                var updateFursuitCollectionGameParticipationConfiguration = Configuration
                    .GetSection("jobs:updateFursuitCollectionGameParticipation").Get<JobConfiguration>();
                var updateLostAndFoundConfiguration = Configuration
                    .GetSection("jobs:updateLostAndFound").Get<JobConfiguration>();

                if (flushPrivateMessageNotificationsConfiguration.Enabled)
                {
                    var flushPrivateMessageNotificationsKey = new JobKey(nameof(FlushPrivateMessageNotificationsJob));
                    q.AddJob<FlushPrivateMessageNotificationsJob>(opts =>
                        opts.WithIdentity(flushPrivateMessageNotificationsKey));
                    q.AddTrigger(t =>
                        t.ForJob(flushPrivateMessageNotificationsKey)
                            .WithSimpleSchedule(s =>
                            {
                                s.WithIntervalInSeconds(flushPrivateMessageNotificationsConfiguration
                                    .SecondsInterval);
                                s.RepeatForever();
                            }));
                }

                if (updateAnnouncementsConfiguration.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(announcementOptions.Url))
                    {
                        logger.LogError( "Update announcements job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateAnnouncementsKey = new JobKey(nameof(UpdateAnnouncementsJob));
                        q.AddJob<UpdateAnnouncementsJob>(opts => opts.WithIdentity(updateAnnouncementsKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateAnnouncementsKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(updateAnnouncementsConfiguration
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (updateDealersConfiguration.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(dealerOptions.Url))
                    {
                        logger.LogError("Update dealers job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateDealersKey = new JobKey(nameof(UpdateDealersJob));
                        q.AddJob<UpdateDealersJob>(opts => opts.WithIdentity(updateDealersKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateDealersKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(updateDealersConfiguration
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (updateEventsConfiguration.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(eventOptions.Url))
                    {
                        logger.LogError("Update events job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateEventsKey = new JobKey(nameof(UpdateEventsJob));
                        q.AddJob<UpdateEventsJob>(opts => opts.WithIdentity(updateEventsKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateEventsKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(updateEventsConfiguration
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (updateFursuitCollectionGameParticipationConfiguration.Enabled)
                {
                    var updateFursuitCollectionGameParticipationKey =
                        new JobKey(nameof(UpdateFursuitCollectionGameParticipationJob));
                    q.AddJob<UpdateFursuitCollectionGameParticipationJob>(opts =>
                        opts.WithIdentity(updateFursuitCollectionGameParticipationKey));
                    q.AddTrigger(t =>
                        t.ForJob(updateFursuitCollectionGameParticipationKey)
                            .WithSimpleSchedule(s =>
                            {
                                s.WithIntervalInSeconds(updateFursuitCollectionGameParticipationConfiguration
                                    .SecondsInterval);
                                s.RepeatForever();
                            }));
                }

                if (updateLostAndFoundConfiguration.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(lassieOptions.BaseApiUrl))
                    {
                        logger.LogError("Update lost and found job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateLostAndFoundKey = new JobKey(nameof(UpdateLostAndFoundJob));
                        q.AddJob<UpdateLostAndFoundJob>(opts => opts.WithIdentity(updateLostAndFoundKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateLostAndFoundKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(updateLostAndFoundConfiguration
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            // Add Mapster for mapping DTOs
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Default.PreserveReference(true);
            typeAdapterConfig.Scan(typeof(Startup).Assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            CidRouteBaseAttribute.Value = globalOptions.ConventionIdentifier;
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime appLifetime
        )
        {
            var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
            Log.Logger = loggerConfiguration.CreateLogger();

            loggerFactory
                .AddSerilog();

            _logger = loggerFactory.CreateLogger(GetType());
            _logger.LogInformation($"Logging commences");

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseCors("CorsPolicy");
            app.UseAuthentication();

            app.UseRewriter(new RewriteOptions()
                .AddRewrite("(.*)/apple-app-site-association", "$1/apple-app-site-association.json", true)
            );

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("IPAddress",
                           context.Request.Headers.ContainsKey("X-Forwarded-For")
                               ? context.Request.Headers["X-Forwarded-For"].ToString()
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

            _logger.LogInformation($"Startup complete ({env.EnvironmentName})");
        }
    }
}
