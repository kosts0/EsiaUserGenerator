using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EsiaUserGenerator.Db.UoW;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Подтягиваем appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.Development.json")
            .Build();

        var connStr = config.GetConnectionString("DefaultConnection");

        builder.UseNpgsql(connStr);

        return new ApplicationDbContext(builder.Options);
    }
}