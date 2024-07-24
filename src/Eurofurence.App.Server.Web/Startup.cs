﻿using Autofac;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
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
using Serilog.Events;
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
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Mapster;
using MapsterMapper;

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
            var conventionSettings = ConventionSettings.FromConfiguration(Configuration);

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

            JsonSerializerOptions serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(), new JsonDateTimeConverter() }
            };

            services.AddSingleton(s => serializerOptions);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("api", new OpenApiInfo
                {
                    Version = conventionSettings.ConventionIdentifier,
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
                        Url = $"/{conventionSettings.ConventionIdentifier}"
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

            services.Configure<QrCodeConfiguration>(Configuration.GetSection("QrCode"));

            services.Configure<IdentityOptions>(Configuration.GetSection("Identity"));
            services.Configure<AuthorizationOptions>(Configuration.GetSection("Authorization"));
            services.ConfigureOptions<ConfigureOAuth2IntrospectionOptions>();

            services.AddTransient<IClaimsTransformation, RolesClaimsTransformation>();
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options =>
                {
                    options.EnableCaching = true;
                });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.Configure<LoggerFilterOptions>(options => { options.MinLevel = LogLevel.Trace; });

            services.AddDbContextPool<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("Eurofurence");

                var serverVersionString = Environment.GetEnvironmentVariable("MYSQL_VERSION");
                ServerVersion serverVersion;
                if (string.IsNullOrEmpty(serverVersionString) || !ServerVersion.TryParse(serverVersionString, out serverVersion))
                {
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }

                options.UseMySql(
                    connectionString,
                    serverVersion,
                    mySqlOptions => mySqlOptions.UseMicrosoftJson());
            });

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

                if (updateDealersConfiguration.Enabled)
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

                if (updateEventsConfiguration.Enabled)
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
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            var builder = new ContainerBuilder();
            // Add Mapster for mapping DTOs
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Default.PreserveReference(true);
            typeAdapterConfig.Scan(typeof(Startup).Assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            builder.Build();

            CidRouteBaseAttribute.Value = conventionSettings.ConventionIdentifier;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new Services.DependencyResolution.AutofacModule(Configuration));
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime appLifetime
        )
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

            var consoleLogLevel = env.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;

            loggerConfiguration
                .WriteTo.Console(
                    restrictedToMinimumLevel: consoleLogLevel,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{IPAddress}] [{Level}] {Message}{NewLine}{Exception}");

            if (!string.IsNullOrEmpty(Configuration["auditLog"]))
            {
                loggerConfiguration
                    .WriteTo
                    .Logger(lc =>
                        lc.Filter
                            .ByIncludingOnly($"EventId.Id = {LogEvents.Audit.Id}")
                            .WriteTo.File(Configuration["auditLog"],
                                restrictedToMinimumLevel: LogEventLevel.Verbose,
                                outputTemplate:
                                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{IPAddress}] [{Level}] {Message}{NewLine}{Exception}"
                            )
                    );
            }

            var cgc = new CollectionGameConfiguration();
            Configuration.GetSection(CollectionGameConfiguration.CollectionGame).Bind(cgc);

            if (!string.IsNullOrEmpty(cgc.LogFile))
            {
                loggerConfiguration
                    .WriteTo
                    .Logger(lc =>
                        lc.Filter
                            .ByIncludingOnly($"EventId.Id = {LogEvents.CollectionGame.Id}")
                            .WriteTo.File(cgc.LogFile, (LogEventLevel)cgc.LogLevel)
                    );
            }
            
            if (!string.IsNullOrEmpty(Configuration["importLog"]))
            {
                loggerConfiguration
                    .WriteTo
                    .Logger(lc =>
                        lc.Filter
                            .ByIncludingOnly($"EventId.Id = {LogEvents.Import.Id}")
                            .WriteTo.File(Configuration["importLog"],
                                restrictedToMinimumLevel: LogEventLevel.Verbose
                            )
                    );
            }
            
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