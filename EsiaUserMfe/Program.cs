
using System.Text.Json;
using EsiaUserMfe;
using EsiaUserMfe.Service;
using EsiaUserMfe.Service.Interface;
using EsiaUserMfe.Utils;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// Указываем корневой компонент
builder.RootComponents.Add<App>("#app");

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true);

builder.Services.AddScoped(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["BffUrl"])
    };
});


builder.Services.AddMudServices();
builder.Services.AddHttpClient<IApiClient, RestService>(client => client.BaseAddress =new Uri(builder.Configuration["BffUrl"]) ); 

await builder.Build().RunAsync();