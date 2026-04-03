using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Summary).IsRequired();

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ConciergeApp)
            .WithMany()
            .HasForeignKey(x => x.ConciergeAppId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssignedMember)
            .WithMany(x => x.AssignedTickets)
            .HasForeignKey(x => x.AssignedMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.ProjectId, x.Status, x.Priority });
        builder.HasIndex(x => new { x.ExternalSystemType, x.ExternalId });
    }
}
