using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Services.Announcements;
using Eurofurence.App.Server.Services.ArtistsAlley;
using Eurofurence.App.Server.Services.ArtShow;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Dealers;
using Eurofurence.App.Server.Services.Events;
using Eurofurence.App.Server.Services.Identity;
using Eurofurence.App.Server.Services.Images;
using Eurofurence.App.Server.Services.Knowledge;
using Eurofurence.App.Server.Services.Lassie;
using Eurofurence.App.Server.Services.LostAndFound;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.Passes;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Services.QrCode;
using Eurofurence.App.Server.Services.Sanitization;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Services.Users;
using Eurofurence.App.Server.Services.Validation;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Identity;
using Eurofurence.App.Server.Web.Jobs;
using Eurofurence.App.Server.Web.Swagger;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;
using System;

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
            services.AddTransient<IEventFavoriteStatisticsService, EventFavoriteStatisticsService>();
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
            services
                .AddTransient<IPushNotificationChannelStatisticsService, PushNotificationChannelStatisticsService>();
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
            services.AddTransient<IPassService, PassService>();

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

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                options.AddSecurityDefinition(ApiKeyAuthenticationDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Name = ApiKeyAuthenticationDefaults.HeaderName,
                        Description = "Authenticate with a static API key",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });

                //options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Server.Web.xml");
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Domain.Model.xml");

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

        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, ILogger logger,
            JobsOptions jobsOptions, LassieOptions lassieOptions, DealerOptions dealerOptions,
            AnnouncementOptions announcementOptions, EventOptions eventOptions, ArtistAlleyOptions artistAlleyOptions)
        {
            services.AddQuartz(q =>
            {
                q.AddJobListener<SentryJobListener>(GroupMatcher<JobKey>.AnyGroup());

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
                    if (string.IsNullOrWhiteSpace(announcementOptions.Url) || !announcementOptions.Url.CheckIsValidHttpsUrl())
                    {
                        logger.Error("Update announcements job cannot be enabled: Announcements.Url must be a valid HTTPS URL");
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
                    if (string.IsNullOrWhiteSpace(dealerOptions.Url) || !dealerOptions.Url.CheckIsValidHttpsUrl())
                    {
                        logger.Error("Update dealers job cannot be enabled: Dealers.Url must be a valid HTTPS URL.");
                    }
                    else if (string.IsNullOrWhiteSpace(dealerOptions.User))
                    {
                        logger.Error("Update dealers job cannot be enabled: Dealers.User must not be empty.");
                    }
                    else if (string.IsNullOrWhiteSpace(dealerOptions.Password))
                    {
                        logger.Error("Update dealers job cannot be enabled: Dealers.Password must not be empty.");
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
                    if (
                        string.IsNullOrWhiteSpace(eventOptions.ApiUrl) ||
                        !eventOptions.ApiUrl.CheckIsValidHttpsUrl()
                        )
                    {
                        logger.Error("Update events job cannot be enabled: Events.ApiUrl must be a valid HTTPS URL.");
                    }
                    else if (string.IsNullOrWhiteSpace(eventOptions.ApiKey))
                    {
                        logger.Error("Update events job cannot be enabled: Events.ApiKey must not be empty.");
                    }
                    else if (string.IsNullOrWhiteSpace(eventOptions.EventSlug))
                    {
                        logger.Error("Update events job cannot be enabled: Events.EventSlug must not be empty.");
                    }
                    else if (string.IsNullOrWhiteSpace(eventOptions.DefaultLocale))
                    {
                        logger.Error("Update events job cannot be enabled: Events.DefaultLocale must not be empty.");
                    }
                    else if (!int.IsPositive(eventOptions.InternalTagId))
                    {
                        logger.Error("Update events job cannot be enabled: Events.InternalTagId must be a positive integer.");
                    }
                    else if (!int.IsPositive(eventOptions.InternalTrackId))
                    {
                        logger.Error("Update events job cannot be enabled: Events.InternalTrackId must be a positive integer.");
                    }
                    else if (eventOptions.EventDays.Count == 0)
                    {
                        logger.Error("Update events job cannot be enabled: Events.EventDays must contain one or more elements.");
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

                if (jobsOptions.UpdateEventFavoriteStatistics.Enabled)
                {
                    var updateEventFavoriteStatisticsKey = new JobKey(nameof(UpdateEventFavoriteStatisticsJob));
                    q.AddJob<UpdateEventFavoriteStatisticsJob>(opts =>
                        opts.WithIdentity(updateEventFavoriteStatisticsKey));
                    q.AddTrigger(t =>
                        t.ForJob(updateEventFavoriteStatisticsKey)
                            .WithSimpleSchedule(s =>
                            {
                                s.WithIntervalInSeconds(jobsOptions.UpdateEventFavoriteStatistics
                                    .SecondsInterval);
                                s.RepeatForever();
                            }));
                }

                if (jobsOptions.UpdateLostAndFound.Enabled)
                {
                    if (string.IsNullOrWhiteSpace(lassieOptions.BaseApiUrl) || !lassieOptions.BaseApiUrl.CheckIsValidHttpsUrl())
                    {
                        logger.Error("Update lost and found job cannot be enabled: Lassie.BaseApiUrl must be a valid HTTPS URL.");
                    }
                    else if (string.IsNullOrWhiteSpace(lassieOptions.ApiKey))
                    {
                        logger.Error("Update lost and found job cannot be enabled: Lassie.ApiKey must not be empty.");
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

                if (jobsOptions.DeleteExpiredArtistAlleyRegistrations.Enabled)
                {
                    if (artistAlleyOptions.ExpirationTimeInHours == null)
                    {
                        logger.Error("Delete Expired Artist Alley Registrations job cannot be enabled: Artist alley ExpirationTimeInHours is not configured. Artist alley registrations will not expire");
                    }
                    else
                    {
                        var deleteExpiredArtistAlleyRegistrationsKey = new JobKey(nameof(DeleteExpiredArtistAlleyRegistrationsJob));
                        q.AddJob<DeleteExpiredArtistAlleyRegistrationsJob>(opts => opts.WithIdentity(deleteExpiredArtistAlleyRegistrationsKey));
                        q.AddTrigger(t =>
                            t.ForJob(deleteExpiredArtistAlleyRegistrationsKey)
                                .WithSimpleSchedule(s =>
                                {
                                    s.WithIntervalInSeconds(jobsOptions.DeleteExpiredArtistAlleyRegistrations
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
            typeAdapterConfig.Scan(typeof(Domain.Model.IAssemblyMarker).Assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        public static IServiceCollection AddFirebase(this IServiceCollection services, ILogger logger, FirebaseOptions firebaseOptions)
        {
            if (string.IsNullOrEmpty(firebaseOptions.GoogleServiceCredentialKeyFile))
                return services;

            if (!System.IO.File.Exists(firebaseOptions.GoogleServiceCredentialKeyFile))
            {
                logger.Error($"Google credentials file for Firebase was not found: {firebaseOptions.GoogleServiceCredentialKeyFile}");
                return services;
            }

            var credential = CredentialFactory.FromFile<ServiceAccountCredential>(firebaseOptions.GoogleServiceCredentialKeyFile)
                                  .ToGoogleCredential();

            if (FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential
                });
            }

            return services;
        }
    }
}