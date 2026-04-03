using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionMetadataAndControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "user_sessions",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeenAt",
                table: "user_sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "RevokedReason",
                table: "user_sessions",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "user_sessions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId_RevokedAt",
                table: "user_sessions",
                columns: new[] { "UserId", "RevokedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_sessions_UserId_RevokedAt",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "user_sessions");
        }
    }
}
