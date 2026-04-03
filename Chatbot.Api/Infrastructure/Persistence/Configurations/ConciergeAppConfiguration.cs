using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class ConciergeAppConfiguration : IEntityTypeConfiguration<ConciergeApp>
{
    public void Configure(EntityTypeBuilder<ConciergeApp> builder)
    {
        builder.ToTable("concierge_apps");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.WelcomeMessage).HasMaxLength(2048);
        builder.Property(x => x.FaqScope).HasMaxLength(2048);
        builder.Property(x => x.BusinessHours).HasMaxLength(256);
        builder.Property(x => x.ChannelLabel).HasMaxLength(128);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.ConciergeApps)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ConciergeApps)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PrimaryAiMember)
            .WithMany()
            .HasForeignKey(x => x.PrimaryAiMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ProjectId, x.Name });
    }
}
