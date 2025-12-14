using EsiaUserGenerator.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RequestHistoryConfiguration : IEntityTypeConfiguration<RequestHistory>
{
    public void Configure(EntityTypeBuilder<RequestHistory> builder)
    {
        builder.ToTable("RequestHistory");

        builder.HasKey(x => x.RequestId);

        builder.Property(x => x.JsonRequest)
            .IsRequired();
        builder.OwnsOne(e => e.GeneratedUserInfo, owned =>
        {
            owned.ToJson();
            owned.OwnsOne(o => o.Documents, docs =>
            {

                docs.OwnsMany(d => d.Elements);
            });
        });
    }
}