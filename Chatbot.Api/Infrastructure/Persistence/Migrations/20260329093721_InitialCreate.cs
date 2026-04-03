using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    BrandName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teams_users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalSystemType = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalRef = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customers_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalSystemType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    AuthConfig = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_integration_connections_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberType = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_members_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_members_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ai_member_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ResponsibilitySummary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    SystemPrompt = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedTools = table.Column<string>(type: "TEXT", nullable: true),
                    KnowledgeScope = table.Column<string>(type: "TEXT", nullable: true),
                    IsAutonomous = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_member_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_member_profiles_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LeadMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalSystemType = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalRef = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects_members_LeadMemberId",
                        column: x => x.LeadMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concierge_apps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    ServiceScope = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryAiMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TicketCreationPolicy = table.Column<string>(type: "TEXT", nullable: true),
                    HumanHandoffPolicy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concierge_apps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concierge_apps_members_PrimaryAiMemberId",
                        column: x => x.PrimaryAiMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_concierge_apps_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_concierge_apps_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConciergeAppId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversations_concierge_apps_ConciergeAppId",
                        column: x => x.ConciergeAppId,
                        principalTable: "concierge_apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversations_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_conversations_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantType = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SenderName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversation_messages_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversation_messages_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConciergeAppId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedMemberId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalSystemType = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalRef = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_concierge_apps_ConciergeAppId",
                        column: x => x.ConciergeAppId,
                        principalTable: "concierge_apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tickets_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tickets_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tickets_members_AssignedMemberId",
                        column: x => x.AssignedMemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tickets_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tickets_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_member_profiles_MemberId",
                table: "ai_member_profiles",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concierge_apps_PrimaryAiMemberId",
                table: "concierge_apps",
                column: "PrimaryAiMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_concierge_apps_ProjectId_Name",
                table: "concierge_apps",
                columns: new[] { "ProjectId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_concierge_apps_TeamId",
                table: "concierge_apps",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_ConversationId_CreatedAt",
                table: "conversation_messages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_MemberId",
                table: "conversation_messages",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_ConciergeAppId_CreatedAt",
                table: "conversations",
                columns: new[] { "ConciergeAppId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_CustomerId",
                table: "conversations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_TeamId",
                table: "conversations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_ExternalSystemType_ExternalId",
                table: "customers",
                columns: new[] { "ExternalSystemType", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_TeamId_Email",
                table: "customers",
                columns: new[] { "TeamId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_integration_connections_TeamId_ExternalSystemType_Name",
                table: "integration_connections",
                columns: new[] { "TeamId", "ExternalSystemType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_TeamId_DisplayName",
                table: "members",
                columns: new[] { "TeamId", "DisplayName" });

            migrationBuilder.CreateIndex(
                name: "IX_members_TeamId_UserId",
                table: "members",
                columns: new[] { "TeamId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_members_UserId",
                table: "members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ExternalSystemType_ExternalId",
                table: "projects",
                columns: new[] { "ExternalSystemType", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_projects_LeadMemberId",
                table: "projects",
                column: "LeadMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_TeamId_Name",
                table: "projects",
                columns: new[] { "TeamId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_teams_OwnerUserId_Name",
                table: "teams",
                columns: new[] { "OwnerUserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_AssignedMemberId",
                table: "tickets",
                column: "AssignedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ConciergeAppId",
                table: "tickets",
                column: "ConciergeAppId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ConversationId",
                table: "tickets",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_CustomerId",
                table: "tickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ExternalSystemType_ExternalId",
                table: "tickets",
                columns: new[] { "ExternalSystemType", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ProjectId_Status_Priority",
                table: "tickets",
                columns: new[] { "ProjectId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_TeamId",
                table: "tickets",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_member_profiles");

            migrationBuilder.DropTable(
                name: "conversation_messages");

            migrationBuilder.DropTable(
                name: "integration_connections");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "concierge_apps");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
