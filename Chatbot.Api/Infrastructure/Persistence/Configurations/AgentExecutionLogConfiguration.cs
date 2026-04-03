using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chatbot.Api.Infrastructure.Persistence.Configurations;

public class AgentExecutionLogConfiguration : IEntityTypeConfiguration<AgentExecutionLog>
{
    public void Configure(EntityTypeBuilder<AgentExecutionLog> builder)
    {
        builder.ToTable("agent_execution_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ToolName).IsRequired();
        builder.Property(x => x.ToolCategory).IsRequired();
        builder.Property(x => x.BoundarySummary).IsRequired();
        builder.Property(x => x.InputSummary).IsRequired();
        builder.Property(x => x.OutputSummary).IsRequired();

        builder.HasOne(x => x.WorkflowStep)
            .WithMany(x => x.ExecutionLogs)
            .HasForeignKey(x => x.WorkflowStepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.WorkflowStepId, x.ToolName });
    }
}
