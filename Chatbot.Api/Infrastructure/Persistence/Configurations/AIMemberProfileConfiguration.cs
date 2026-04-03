using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class AIMemberProfileConfiguration : IEntityTypeConfiguration<AIMemberProfile>
{
    public void Configure(EntityTypeBuilder<AIMemberProfile> builder)
    {
        builder.ToTable("ai_member_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobTitle).IsRequired();
        builder.Property(x => x.ResponsibilitySummary).IsRequired();

        builder.HasIndex(x => x.MemberId).IsUnique();
    }
}
