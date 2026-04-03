using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired();

        builder.HasOne(x => x.Team)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LeadMember)
            .WithMany(x => x.LeadProjects)
            .HasForeignKey(x => x.LeadMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TeamId, x.Name });
        builder.HasIndex(x => new { x.ExternalSystemType, x.ExternalId });
    }
}
