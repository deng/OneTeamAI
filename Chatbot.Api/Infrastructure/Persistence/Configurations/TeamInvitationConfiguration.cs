using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class TeamInvitationConfiguration : IEntityTypeConfiguration<TeamInvitation>
{
    public void Configure(EntityTypeBuilder<TeamInvitation> builder)
    {
        builder.ToTable("team_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired();

        builder.HasOne(x => x.Team)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InvitedByUser)
            .WithMany(x => x.SentInvitations)
            .HasForeignKey(x => x.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcceptedByUser)
            .WithMany(x => x.AcceptedInvitations)
            .HasForeignKey(x => x.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TeamId, x.Email, x.Status });
        builder.HasIndex(x => x.Email);
    }
}
