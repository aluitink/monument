using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using monument.api.client;
using monument.ui;
using monument.ui.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Client configuration settings
builder.Services.AddOptions<MonumentSettings>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("Monument").Bind(settings);
    });


builder.Services
    .AddAuthorizationCore()
    .AddScoped<AuthenticationStateProvider, MonumentAuthenticationStateProvider>();

// HttpClient for MonumentApiClient
builder.Services.AddHttpClient("api", (sp, client) =>
{
    var appSettings = sp.GetRequiredService<IOptions<MonumentSettings>>().Value;
    if (string.IsNullOrWhiteSpace(appSettings.ApiUrl))
        client.BaseAddress = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "api");
    else
        client.BaseAddress = new Uri(appSettings.ApiUrl);
});

// MonumentApiClient
builder.Services.AddTransient<MonumentApiClient>((sp) =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("api");
    return new MonumentApiClient(httpClient) {  BaseUrl = httpClient?.BaseAddress?.ToString() };
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
