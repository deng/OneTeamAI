using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiMemberTemplateLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_member_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ResponsibilitySummary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PermissionBoundary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    SystemPrompt = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedTools = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutableActions = table.Column<string>(type: "TEXT", nullable: true),
                    KnowledgeScope = table.Column<string>(type: "TEXT", nullable: true),
                    IsAutonomous = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_member_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_member_templates_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_member_templates_Key",
                table: "ai_member_templates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_member_templates_TeamId_IsEnabled_SortOrder",
                table: "ai_member_templates",
                columns: new[] { "TeamId", "IsEnabled", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_member_templates");
        }
    }
}
