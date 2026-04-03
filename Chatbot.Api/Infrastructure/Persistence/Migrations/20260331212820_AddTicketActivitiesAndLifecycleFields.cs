using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketActivitiesAndLifecycleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "tickets",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueAt",
                table: "tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastActivityAt",
                table: "tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ticket_activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActorMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActivityType = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ticket_activities_members_ActorMemberId",
                        column: x => x.ActorMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ticket_activities_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_activities_tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_activities_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_activities_ActorMemberId",
                table: "ticket_activities",
                column: "ActorMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_activities_ActorUserId",
                table: "ticket_activities",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_activities_TeamId",
                table: "ticket_activities",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_activities_TicketId_CreatedAt",
                table: "ticket_activities",
                columns: new[] { "TicketId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_activities");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "DueAt",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "tickets");
        }
    }
}
