using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_execution_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ToolName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ToolCategory = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BoundarySummary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    InputSummary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    OutputSummary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    WasAllowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_execution_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_execution_logs_agent_workflow_steps_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalTable: "agent_workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_execution_logs_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_execution_logs_MemberId",
                table: "agent_execution_logs",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_execution_logs_WorkflowStepId_ToolName",
                table: "agent_execution_logs",
                columns: new[] { "WorkflowStepId", "ToolName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_execution_logs");
        }
    }
}
