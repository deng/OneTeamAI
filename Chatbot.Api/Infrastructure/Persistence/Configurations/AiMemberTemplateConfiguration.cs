using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class AiMemberTemplateConfiguration : IEntityTypeConfiguration<AiMemberTemplate>
{
    public void Configure(EntityTypeBuilder<AiMemberTemplate> builder)
    {
        builder.ToTable("ai_member_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).IsRequired();
        builder.Property(x => x.Label).IsRequired();
        builder.Property(x => x.DisplayName).IsRequired();
        builder.Property(x => x.JobTitle).IsRequired();
        builder.Property(x => x.ResponsibilitySummary).IsRequired();
        builder.Property(x => x.IsEnabled).HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => new { x.TeamId, x.IsEnabled, x.SortOrder });

        builder.HasOne(x => x.Team)
            .WithMany(x => x.AiMemberTemplates)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
