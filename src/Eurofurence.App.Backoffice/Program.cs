using Eurofurence.App.Backoffice;
using Eurofurence.App.Backoffice.Authentication;
using Eurofurence.App.Backoffice.Services;
using Microsoft.AspNetCore.Components.Web;
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

builder.Services.AddScoped<TokenAuthorizationMessageHandler>();
builder.Services.AddHttpClient("api", options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiUrl") ?? string.Empty);
}).AddHttpMessageHandler<TokenAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("profile");
});

var authSettings = builder.Configuration.GetSection("Authorization").Get<AuthorizationSettings>() ?? new AuthorizationSettings();

builder.Services.AddAuthorizationCore(config =>
{
    config.AddPolicy("RequireKnowledgeBaseEditor", policy =>
        policy.RequireAssertion(context =>
            context.User.Claims.Any(c =>
                c.Type == "groups" && authSettings.KnowledgeBaseEditor.Any(group => c.Value.Contains(group))
                )
            )
        );
}
);

await builder.Build().RunAsync();