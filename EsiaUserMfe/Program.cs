using EsiaUserMfe;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// Указываем корневой компонент
builder.RootComponents.Add<App>("#app");

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["BffUrl"])
});

await builder.Build().RunAsync();