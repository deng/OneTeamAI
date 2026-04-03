using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(128);
        builder.Property(x => x.SourceLabel).HasMaxLength(128);
        builder.Property(x => x.Tags).HasMaxLength(256);
        builder.Property(x => x.Notes).HasMaxLength(2048);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.TeamId, x.Email });
        builder.HasIndex(x => new { x.TeamId, x.ProjectId });
        builder.HasIndex(x => new { x.ExternalSystemType, x.ExternalId });
    }
}
