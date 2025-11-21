using EsiaUserGenerator.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RequestHistoryConfiguration : IEntityTypeConfiguration<RequestHistory>
{
    public void Configure(EntityTypeBuilder<RequestHistory> builder)
    {
        builder.ToTable("RequestHistory");

        builder.HasKey(x => new { x.UserId, x.RequestId });

        builder.Property(x => x.JsonRequest)
            .IsRequired();
    }
}