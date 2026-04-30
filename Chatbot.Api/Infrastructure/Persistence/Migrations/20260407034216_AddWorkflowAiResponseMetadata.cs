using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAiResponseMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputRawResponse",
                table: "agent_workflow_steps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputSchemaVersion",
                table: "agent_workflow_steps",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryRawResponse",
                table: "agent_workflow_runs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummarySchemaVersion",
                table: "agent_workflow_runs",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputRawResponse",
                table: "agent_workflow_steps");

            migrationBuilder.DropColumn(
                name: "OutputSchemaVersion",
                table: "agent_workflow_steps");

            migrationBuilder.DropColumn(
                name: "SummaryRawResponse",
                table: "agent_workflow_runs");

            migrationBuilder.DropColumn(
                name: "SummarySchemaVersion",
                table: "agent_workflow_runs");
        }
    }
}
