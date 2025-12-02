using EsiaUserGenerator.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RequestHistoryConfiguration : IEntityTypeConfiguration<UserRequestHistory>
{
    public void Configure(EntityTypeBuilder<UserRequestHistory> builder)
    {
        builder.ToTable("RequestHistory");

        builder.HasKey(x => x.RequestId);

        builder.Property(x => x.JsonRequest)
            .IsRequired();

        builder.HasMany(x => x.GeneratedPofiles)
            .WithOne(x => x.RequestData)
            .HasForeignKey(x => x.CreatedRequestId);
    }
}