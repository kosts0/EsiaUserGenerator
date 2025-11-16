using EsiaUserMfe;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// Указываем корневой компонент
builder.RootComponents.Add<App>("#app");


// HttpClient – наша связь с API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://bff.mycompany.com/") // URL твоего BFF
});

await builder.Build().RunAsync();