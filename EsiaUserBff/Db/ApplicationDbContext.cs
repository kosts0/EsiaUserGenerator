using EsiaUserGenerator.Db.Configuration;
using EsiaUserGenerator.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace EsiaUserGenerator.Db;

public class ApplicationDbContext : DbContext
{
    public DbSet<EsiaUser> EsiaUsers => Set<EsiaUser>();
    public DbSet<CreatedHistory> CreatedHistory => Set<CreatedHistory>();
    public DbSet<RequestHistory> RequestHistory => Set<RequestHistory>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EsiaUserConfiguration());
        modelBuilder.ApplyConfiguration(new CreatedHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new RequestHistoryConfiguration());
    }
}
