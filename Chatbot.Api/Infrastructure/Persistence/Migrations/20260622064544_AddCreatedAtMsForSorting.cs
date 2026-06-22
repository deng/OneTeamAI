using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtMsForSorting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "user_sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "user_sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "ticket_activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "ticket_activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "team_invitations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "team_invitations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "project_members",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "project_members",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "members",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "members",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "integration_connections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "integration_connections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "conversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "conversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "conversation_messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "conversation_messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "concierge_apps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "concierge_apps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "audit_logs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "audit_logs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "ai_member_templates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "ai_member_templates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "ai_member_profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "ai_member_profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "agent_workflow_steps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "agent_workflow_steps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "agent_workflow_runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "agent_workflow_runs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAtMs",
                table: "agent_execution_logs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAtMs",
                table: "agent_execution_logs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "ticket_activities");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "ticket_activities");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "team_invitations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "team_invitations");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "project_members");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "project_members");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "members");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "members");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "integration_connections");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "integration_connections");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "ai_member_templates");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "ai_member_templates");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "ai_member_profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "ai_member_profiles");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "agent_workflow_steps");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "agent_workflow_steps");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "agent_workflow_runs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "agent_workflow_runs");

            migrationBuilder.DropColumn(
                name: "CreatedAtMs",
                table: "agent_execution_logs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtMs",
                table: "agent_execution_logs");
        }
    }
}
