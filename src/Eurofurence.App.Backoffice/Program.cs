using Eurofurence.App.Backoffice;
using Eurofurence.App.Backoffice.Authentication;
using Eurofurence.App.Backoffice.Services;
using Eurofurence.App.Domain.Model.Identity;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<SentryBlazorOptions>(builder.Configuration.GetSection("Sentry"));

builder.UseSentry(options =>
{
    options.Dsn ??= SentryConstants.DisableSdkDsnValue;
});

builder.Services.AddMudServices();

builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IArtistAlleyService, ArtistAlleyService>();
builder.Services.AddScoped<IDealerService, DealerService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<TokenAuthorizationMessageHandler>();
builder.Services.AddHttpClient("api", options =>
{
    options.BaseAddress = new Uri($"{builder.Configuration.GetValue<string>("BackendBaseUrl")?.TrimEnd('/') ?? string.Empty}/Api/");
}).AddHttpMessageHandler<TokenAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("profile");
}).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, RemoteUserAccount, BackendAccountClaimsFactory>();

builder.Services.AddAuthorizationCore(config =>
{
    config.AddPolicy("RequireKnowledgeBaseEditor", policy =>
        policy.RequireRole([IdentityRoles.KnowledgeBaseEditor, IdentityRoles.Admin])
        );
    config.AddPolicy("RequireArtistAlleyModerator", policy =>
        policy.RequireRole([IdentityRoles.ArtistAlleyModerator, IdentityRoles.ArtistAlleyAdmin, IdentityRoles.Admin])
        );
    config.AddPolicy("RequireArtistAlleyAdmin", policy =>
        policy.RequireRole([IdentityRoles.ArtistAlleyAdmin, IdentityRoles.Admin])
        );
}
);

await builder.Build().RunAsync();