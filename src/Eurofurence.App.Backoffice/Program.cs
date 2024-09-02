using System.Security.Claims;
using Eurofurence.App.Backoffice;
using Eurofurence.App.Backoffice.Authentication;
using Eurofurence.App.Backoffice.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IArtistAlleyService, ArtistAlleyService>();
builder.Services.AddScoped<IFursuitService, FursuitService>();
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
        policy.RequireRole(["KnowledgeBaseEditor", "Admin"])
        );
    config.AddPolicy("RequireArtistAlleyModerator", policy =>
        policy.RequireRole(["ArtistAlleyModerator", "ArtistAlleyAdmin", "Admin"])
        );
    config.AddPolicy("RequireArtistAlleyAdmin", policy =>
        policy.RequireRole(["ArtistAlleyAdmin", "Admin"])
        );
}
);

await builder.Build().RunAsync();