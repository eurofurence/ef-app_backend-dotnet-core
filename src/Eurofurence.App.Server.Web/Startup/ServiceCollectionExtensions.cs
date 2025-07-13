using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Announcements;
using Eurofurence.App.Server.Services.ArtistsAlley;
using Eurofurence.App.Server.Services.ArtShow;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Dealers;
using Eurofurence.App.Server.Services.Events;
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
using Microsoft.Extensions.DependencyInjection;
using Eurofurence.App.Server.Web.Identity;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.OpenApi.Models;
using System;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Eurofurence.App.Server.Services.Identity;
using Eurofurence.App.Server.Services.QrCode;
using Eurofurence.App.Server.Web.Jobs;
using Quartz;
using Mapster;
using MapsterMapper;
using Serilog;

namespace Eurofurence.App.Server.Web.Startup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IAgentClosingResultService, AgentClosingResultService>();
            services.AddTransient<IAnnouncementService, AnnouncementService>();
            services.AddTransient<IDealerService, DealerService>();
            services.AddTransient<IEventConferenceDayService, EventConferenceDayService>();
            services.AddTransient<IEventConferenceRoomService, EventConferenceRoomService>();
            services.AddTransient<IEventConferenceTrackService, EventConferenceTrackService>();
            services.AddTransient<IEventFeedbackService, EventFeedbackService>();
            services.AddTransient<IEventService, EventService>();
            services.AddTransient<IPushNotificationChannelManager, PushNotificationChannelManager>();
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
            services.AddTransient<IDealerApiClient, DealerApiClient>();
            services.AddTransient<IDeviceIdentityService, DeviceIdentityService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IArtistAlleyUserPenaltyService, ArtistAlleyUserPenaltyService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<IPrivateMessageQueueService, PrivateMessageQueueService>();

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services, GlobalOptions globalOptions)
        {
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

            return services;
        }

        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, ILogger logger, JobsOptions jobsOptions, LassieOptions lassieOptions, DealerOptions dealerOptions, AnnouncementOptions announcementOptions, EventOptions eventOptions)
        {
            services.AddQuartz(q =>
            {
                if (jobsOptions.FlushPrivateMessageNotifications.Enabled)
                {
                    var flushPrivateMessageNotificationsKey = new JobKey(nameof(FlushPrivateMessageNotificationsJob));
                    q.AddJob<FlushPrivateMessageNotificationsJob>(opts =>
                        opts.WithIdentity(flushPrivateMessageNotificationsKey));
                    q.AddTrigger(t =>
                        t.ForJob(flushPrivateMessageNotificationsKey)
                            .WithSimpleSchedule(s =>
                            {
                                s.WithIntervalInSeconds(jobsOptions.FlushPrivateMessageNotifications
                                    .SecondsInterval);
                                s.RepeatForever();
                            }));
                }

                if (jobsOptions.UpdateAnnouncements.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(announcementOptions.Url))
                    {
                        logger.Error("Update announcements job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateAnnouncementsKey = new JobKey(nameof(UpdateAnnouncementsJob));
                        q.AddJob<UpdateAnnouncementsJob>(opts => opts.WithIdentity(updateAnnouncementsKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateAnnouncementsKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(jobsOptions.UpdateAnnouncements
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (jobsOptions.UpdateDealers.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(dealerOptions.Url))
                    {
                        logger.Error("Update dealers job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateDealersKey = new JobKey(nameof(UpdateDealersJob));
                        q.AddJob<UpdateDealersJob>(opts => opts.WithIdentity(updateDealersKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateDealersKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(jobsOptions.UpdateDealers
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (jobsOptions.UpdateEvents.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(eventOptions.Url))
                    {
                        logger.Error("Update events job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateEventsKey = new JobKey(nameof(UpdateEventsJob));
                        q.AddJob<UpdateEventsJob>(opts => opts.WithIdentity(updateEventsKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateEventsKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(jobsOptions.UpdateEvents
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }

                if (jobsOptions.UpdateLostAndFound.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(lassieOptions.BaseApiUrl))
                    {
                        logger.Error("Update lost and found job can't be added: Empty source url");
                    }
                    else
                    {
                        var updateLostAndFoundKey = new JobKey(nameof(UpdateLostAndFoundJob));
                        q.AddJob<UpdateLostAndFoundJob>(opts => opts.WithIdentity(updateLostAndFoundKey));
                        q.AddTrigger(t =>
                            t.ForJob(updateLostAndFoundKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(jobsOptions.UpdateLostAndFound
                                        .SecondsInterval);
                                    s.RepeatForever();
                                }));
                    }
                }
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Default.PreserveReference(true);
            typeAdapterConfig.Scan(typeof(Program).Assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}
