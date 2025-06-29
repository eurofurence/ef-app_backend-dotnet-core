using System;
using System.Collections.Generic;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Identity;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using dotAPNS.AspNetCore;
using Eurofurence.App.Server.Web.Startup;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Swashbuckle.AspNetCore.SwaggerUI;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.MinIO;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Mapster;

namespace Eurofurence.App.Server.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration);
            Log.Logger = loggerConfiguration.CreateLogger();
            var logger = Log.ForContext<Program>();
            logger.Information($"Logging configured");

            builder.Services.AddLogging(options =>
            {
                options.ClearProviders();
                options.AddSerilog(dispose: true);
            });


            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(30001);
            });

            var connectionString = builder.Configuration.GetConnectionString("Eurofurence");
            var globalOptions = builder.Configuration.GetSection("Global").Get<GlobalOptions>();
            var lassieOptions = builder.Configuration.GetSection("Lassie").Get<LassieOptions>();
            var dealerOptions = builder.Configuration.GetSection("Dealers").Get<DealerOptions>();
            var announcementOptions = builder.Configuration.GetSection("Announcements").Get<AnnouncementOptions>();
            var eventOptions = builder.Configuration.GetSection("Events").Get<EventOptions>();
            var telegramOptions = builder.Configuration.GetSection("Telegram").Get<TelegramOptions>();
            var jobsOptions = builder.Configuration.GetSection("Jobs").Get<JobsOptions>();

            builder.Services.Configure<GlobalOptions>(builder.Configuration.GetSection("Global"));
            builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection("Push:Firebase"));
            builder.Services.Configure<ApnsOptions>(builder.Configuration.GetSection("Push:Apns"));
            builder.Services.Configure<ExpoOptions>(builder.Configuration.GetSection("Push:Expo"));
            builder.Services.Configure<QrCodeOptions>(builder.Configuration.GetSection("QrCode"));
            builder.Services.Configure<ArtistAlleyOptions>(builder.Configuration.GetSection("ArtistAlley"));
            builder.Services.Configure<LassieOptions>(builder.Configuration.GetSection("Lassie"));
            builder.Services.Configure<MinIoOptions>(builder.Configuration.GetSection("MinIo"));
            builder.Services.Configure<DealerOptions>(builder.Configuration.GetSection("Dealers"));
            builder.Services.Configure<AnnouncementOptions>(builder.Configuration.GetSection("Announcements"));
            builder.Services.Configure<EventOptions>(builder.Configuration.GetSection("Events"));
            builder.Services.Configure<MapOptions>(builder.Configuration.GetSection("Maps"));
            builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection("Identity"));
            builder.Services.Configure<AuthorizationOptions>(builder.Configuration.GetSection("Authorization"));
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
            });
            builder.Services.ConfigureOptions<ConfigureOAuth2IntrospectionOptions>();
            builder.Services.Configure<LoggerFilterOptions>(options => { options.MinLevel = LogLevel.Trace; });

            builder.Services.AddServices();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    cpb => cpb.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            builder.Services.AddMvc(options =>
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

            builder.Services.AddSingleton(s => new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(), new JsonDateTimeConverter() }
            });

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddSwagger(globalOptions);

            builder.Services.AddTransient<IClaimsTransformation, RolesClaimsTransformation>();
            builder.Services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options => { options.EnableCaching = true; })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>
                (ApiKeyAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ApiKeys = builder.Configuration.GetSection("ApiKeys")
                            .Get<IList<ApiKeyAuthenticationOptions.ApiKeyOptions>>();
                        foreach (var apiKey in options.ApiKeys ?? [])
                        {
                            logger.Information($"Configured API key for {apiKey.PrincipalName} with roles {string.Join(',', apiKey.Roles)} valid until {apiKey.ValidUntil}.");
                        }
                    });

            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            if (telegramOptions.AccessToken is { Length: > 0 })
            {
                builder.Services.AddTelegramBot(telegramOptions);
            }

            builder.Services.AddDbContextPool<AppDbContext>(options =>
            {
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

            builder.Services.AddApns();

            builder.Services.AddQuartzJobs(logger, jobsOptions, lassieOptions, dealerOptions, announcementOptions, eventOptions);

            builder.Services.AddMapper();

            CidRouteBaseAttribute.Value = globalOptions.ConventionIdentifier;

            var app = builder.Build();

            app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

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
                           context.Request.Headers.TryGetValue("X-Forwarded-For", out var header)
                               ? header.ToString()
                               : context.Connection.RemoteIpAddress?.ToString()))
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

            logger.Information("Compiling type mappings");
            TypeAdapterConfig.GlobalSettings.Compile();

            logger.Information($"Startup complete ({builder.Environment.EnvironmentName})");

            app.Run();
        }
    }
}
