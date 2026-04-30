using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConciergeIntakeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntakeGuidance",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmail",
                table: "concierge_apps",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePhoneNumber",
                table: "concierge_apps",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedPrompts",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntakeGuidance",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "RequireEmail",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "RequirePhoneNumber",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "SuggestedPrompts",
                table: "concierge_apps");
        }
    }
}
