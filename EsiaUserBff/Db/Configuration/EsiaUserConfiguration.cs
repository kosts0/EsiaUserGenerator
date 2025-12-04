using EsiaUserGenerator.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EsiaUserConfiguration : IEntityTypeConfiguration<EsiaUser>
{
    public void Configure(EntityTypeBuilder<EsiaUser> builder)
    {
        builder.ToTable("EsiaUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Oid).IsRequired(false);
        builder.Property(x => x.Login).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Password).IsRequired();

        builder
            .HasOne(e => e.RequestData)
            .WithOne(r => r.User)
            .HasForeignKey<EsiaUser>(e => e.CreatedRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}