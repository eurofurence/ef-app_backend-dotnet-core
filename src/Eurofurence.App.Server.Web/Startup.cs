using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Autofac;
using Autofac.Features.AttributeFilters;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Jobs;
using Eurofurence.App.Server.Web.Swagger;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Web.Identity;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authorization;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
                options.SwaggerDoc("api", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = conventionSettings.ConventionIdentifier,
                    Title = "Eurofurence API for Mobile Apps",
                    Description = "",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Eurofurence IT Department",
                        Email = "it@eurofurence.org",
                        Url = new Uri("https://help.eurofurence.org/contact/it")
                    }
                });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
                });

                //options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Server.Web.xml");
                options.IncludeXmlComments($@"{AppContext.BaseDirectory}/Eurofurence.App.Domain.Model.xml");

                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                options.OperationFilter<BinaryPayloadFilter>();


                if (Environment.GetEnvironmentVariable("CID_IN_API_BASE_PATH") == "1")
                {
                    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer()
                    {
                        Description = "nginx (CID in path)",
                        Url = $"/{conventionSettings.ConventionIdentifier}"
                    });
                }
                else
                {
                    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer()
                    {
                        Description = "local",
                        Url = "/"
                    });
                }

            });

            services.Configure<IdentityOptions>(Configuration.GetSection("Identity"));
            services.ConfigureOptions<ConfigureOAuth2IntrospectionOptions>();

            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.Configure<LoggerFilterOptions>(options =>
            {
                options.MinLevel = LogLevel.Trace;
            });

            var builder = new ContainerBuilder();

            services.AddDbContextPool<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("Eurofurence");
                options.UseMySql(
                    connectionString, 
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.UseMicrosoftJson());
            });
            
            builder.Build();

            CidRouteBaseAttribute.Value = conventionSettings.ConventionIdentifier;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new Services.DependencyResolution.AutofacModule(Configuration));

            builder.RegisterType<UpdateNewsJob>().WithAttributeFiltering().AsSelf();
            builder.RegisterType<UpdateLostAndFoundJob>().WithAttributeFiltering().AsSelf();
            builder.RegisterType<UpdateFursuitCollectionGameParticipationJob>().WithAttributeFiltering().AsSelf();
            builder.RegisterType<FlushPrivateMessageNotificationsJob>().AsSelf();
            builder.Register(c => Configuration.GetSection("jobs:updateNews"))
                .Keyed<IConfiguration>("updateNews").As<IConfiguration>();

        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime appLifetime
        )
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

            if (env.IsDevelopment())
            {
                loggerConfiguration
                    .MinimumLevel.Debug()
                    .WriteTo.Console();
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

            var cgc = new CollectionGameConfiguration();
            Configuration.GetSection(CollectionGameConfiguration.CollectionGame).Bind(cgc);
            
            loggerConfiguration
                .WriteTo
                .Logger(lc =>
                    lc.Filter
                        .ByIncludingOnly($"EventId.Id = {LogEvents.CollectionGame.Id}")
                        .WriteTo.File(cgc.LogFile, (LogEventLevel)cgc.LogLevel)
                );

            Log.Logger = loggerConfiguration.CreateLogger();

            loggerFactory
                .AddSerilog();

            _logger = loggerFactory.CreateLogger(GetType());
            _logger.LogInformation($"Logging commences");

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseCors("CorsPolicy");
            app.UseAuthentication();

            app.UseStaticFiles("/Web/Static");

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