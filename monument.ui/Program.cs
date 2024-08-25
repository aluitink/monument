using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using monument.api.client;
using monument.ui;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Client configuration settings
builder.Services.AddOptions<MonumentSettings>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("Monument").Bind(settings);
    });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddFluentUIComponents();

builder.Services.AddTransient<MonumentApiClient>();

await builder.Build().RunAsync();
