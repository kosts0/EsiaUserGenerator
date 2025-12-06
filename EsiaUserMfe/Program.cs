using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using EsiaUserMfe;
using EsiaUserMfe.Service;
using EsiaUserMfe.Service.Interface;
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


builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    } )
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
builder.Services.AddHttpClient<IApiClient, RestService>(client => client.BaseAddress =new Uri(builder.Configuration["BffUrl"]) ); 

await builder.Build().RunAsync();