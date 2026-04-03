using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiMemberTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutableActions",
                table: "ai_member_profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionBoundary",
                table: "ai_member_profiles",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateKey",
                table: "ai_member_profiles",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutableActions",
                table: "ai_member_profiles");

            migrationBuilder.DropColumn(
                name: "PermissionBoundary",
                table: "ai_member_profiles");

            migrationBuilder.DropColumn(
                name: "TemplateKey",
                table: "ai_member_profiles");
        }
    }
}
