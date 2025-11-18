
using EsiaUserGenerator.Logs;
using EsiaUserGenerator.Service;
using EsiaUserGenerator.Service.Interface;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddHttpClient<IEsiaRegistrationService, EsiaRegistrationService>(client =>
{
    client.BaseAddress = new Uri("https://esia-portal1.test.gosuslugi.ru");
}).AddHttpMessageHandler<LoggingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Disable CORS since angular will be running on port 4200 and the service on port 5258.
    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}



app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();




app.Run();