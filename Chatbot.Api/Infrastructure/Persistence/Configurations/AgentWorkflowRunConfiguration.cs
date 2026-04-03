using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class AgentWorkflowRunConfiguration : IEntityTypeConfiguration<AgentWorkflowRun>
{
    public void Configure(EntityTypeBuilder<AgentWorkflowRun> builder)
    {
        builder.ToTable("agent_workflow_runs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkflowType).IsRequired();
        builder.Property(x => x.Goal).IsRequired();
        builder.Property(x => x.Summary).IsRequired();

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.StartedByMember)
            .WithMany()
            .HasForeignKey(x => x.StartedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.WorkflowRun)
            .HasForeignKey(x => x.WorkflowRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TeamId, x.TicketId, x.CreatedAt });
        builder.HasIndex(x => new { x.TeamId, x.Status, x.WorkflowType });
    }
}
