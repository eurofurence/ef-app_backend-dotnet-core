using Eurofurence.App.Backoffice.Client.Authentication;
using Eurofurence.App.Backoffice.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient("MyApiWithToken", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "http://localhost:7371/api/"); // Replace with your API base URL
    })
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("MyApiWithToken");
});

//builder.Services.AddHttpClient<IKnowledgeService, KnowledgeService>(httpClient =>
//{
//    httpClient.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "http://localhost:7371/api/");
//});

await builder.Build().RunAsync();