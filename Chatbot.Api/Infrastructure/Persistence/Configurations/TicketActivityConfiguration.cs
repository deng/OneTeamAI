using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.ToTable("ticket_activities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Summary).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Detail).HasMaxLength(4096);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActorMember)
            .WithMany()
            .HasForeignKey(x => x.ActorMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.TicketId, x.CreatedAt });
    }
}
