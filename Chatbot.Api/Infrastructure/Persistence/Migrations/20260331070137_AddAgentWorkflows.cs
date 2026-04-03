using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_workflow_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TicketId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StartedByMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WorkflowType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Goal = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_workflow_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_members_StartedByMemberId",
                        column: x => x.StartedByMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_workflow_runs_users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "agent_workflow_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HandoffToMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    InputSummary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    OutputSummary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    HandoffSummary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agent_workflow_steps_agent_workflow_runs_WorkflowRunId",
                        column: x => x.WorkflowRunId,
                        principalTable: "agent_workflow_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_workflow_steps_members_HandoffToMemberId",
                        column: x => x.HandoffToMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_agent_workflow_steps_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_ConversationId",
                table: "agent_workflow_runs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_ProjectId",
                table: "agent_workflow_runs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_RequestedByUserId",
                table: "agent_workflow_runs",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_StartedByMemberId",
                table: "agent_workflow_runs",
                column: "StartedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_TeamId_Status_WorkflowType",
                table: "agent_workflow_runs",
                columns: new[] { "TeamId", "Status", "WorkflowType" });

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_TeamId_TicketId_CreatedAt",
                table: "agent_workflow_runs",
                columns: new[] { "TeamId", "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_runs_TicketId",
                table: "agent_workflow_runs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_steps_HandoffToMemberId",
                table: "agent_workflow_steps",
                column: "HandoffToMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_steps_MemberId",
                table: "agent_workflow_steps",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_workflow_steps_WorkflowRunId_Sequence",
                table: "agent_workflow_steps",
                columns: new[] { "WorkflowRunId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_workflow_steps");

            migrationBuilder.DropTable(
                name: "agent_workflow_runs");
        }
    }
}
