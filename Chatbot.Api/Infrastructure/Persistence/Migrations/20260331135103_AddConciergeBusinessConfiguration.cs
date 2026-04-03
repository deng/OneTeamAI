using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConciergeBusinessConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChannelLabel",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaqScope",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "concierge_apps",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "ChannelLabel",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "FaqScope",
                table: "concierge_apps");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "concierge_apps");
        }
    }
}
