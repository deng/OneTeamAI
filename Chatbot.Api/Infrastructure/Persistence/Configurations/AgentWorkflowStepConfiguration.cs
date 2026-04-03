using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class AgentWorkflowStepConfiguration : IEntityTypeConfiguration<AgentWorkflowStep>
{
    public void Configure(EntityTypeBuilder<AgentWorkflowStep> builder)
    {
        builder.ToTable("agent_workflow_steps");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType).IsRequired();
        builder.Property(x => x.InputSummary).IsRequired();
        builder.Property(x => x.OutputSummary).IsRequired();
        builder.Property(x => x.HandoffSummary).IsRequired();

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.HandoffToMember)
            .WithMany()
            .HasForeignKey(x => x.HandoffToMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.WorkflowRunId, x.Sequence }).IsUnique();
    }
}
