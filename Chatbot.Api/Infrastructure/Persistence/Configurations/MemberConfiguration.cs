using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName).IsRequired();

        builder.HasOne(x => x.Team)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiProfile)
            .WithOne(x => x.Member)
            .HasForeignKey<AIMemberProfile>(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TeamId, x.DisplayName });
        builder.HasIndex(x => new { x.TeamId, x.UserId });
    }
}
