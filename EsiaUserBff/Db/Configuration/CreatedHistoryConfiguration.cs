using EsiaUserGenerator.Db.Models;

namespace EsiaUserGenerator.Db.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CreatedHistoryConfiguration : IEntityTypeConfiguration<CreatedHistory>
{
    public void Configure(EntityTypeBuilder<CreatedHistory> builder)
    {
        builder.ToTable("CreatedHistory");

        builder.HasKey(x => new { x.UserId, x.CreatedRequestId });
    }
}
