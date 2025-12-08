
using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Logs;
using EsiaUserGenerator.Service;
using EsiaUserGenerator.Service.Interface;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        
// Настройка PostgreSQL
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозитории и UnitOfWork
        

        builder.Services.AddScoped<IEsiaUserRepository, EsiaUserRepository>();
        //builder.Services.AddScoped<ICreatedHistoryRepository, CreatedHistoryRepository>();
        builder.Services.AddScoped<IRequestHistoryRepository, RequestHistoryRepository>();
        
        
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Encoder =
                System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        });;
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<LoggingHandler>();
       
        builder.Services.AddScoped<IEsiaRegistrationService, EsiaRegistrationService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IUserProgressTracker, DbUserProgressTracker>();
        //redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect("localhost:6379")
        );
        
        builder.Services.AddSingleton<IRequestStatusStore, RedisRequestStatusStore>();
        builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        //request task worker
      
        builder.Services.AddHostedService<WorkerBackgroundService>();
            
            var app = builder.Build();

// Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                // Disable CORS since angular will be running on port 4200 and the service on port 5258.
                app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
            else
            {
                app.UseHttpsRedirection();
            }
            
            
            app.UseStaticFiles();
            app.MapControllers();



            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }
            app.Run();
    }
}