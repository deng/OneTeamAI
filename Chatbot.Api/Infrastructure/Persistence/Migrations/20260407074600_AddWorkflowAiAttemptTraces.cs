using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAiAttemptTraces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputAttemptTrace",
                table: "agent_workflow_steps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryAttemptTrace",
                table: "agent_workflow_runs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputAttemptTrace",
                table: "agent_workflow_steps");

            migrationBuilder.DropColumn(
                name: "SummaryAttemptTrace",
                table: "agent_workflow_runs");
        }
    }
}
