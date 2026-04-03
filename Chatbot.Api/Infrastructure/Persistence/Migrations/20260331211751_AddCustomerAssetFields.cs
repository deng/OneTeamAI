using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatbot.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAssetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FollowUpStatus",
                table: "customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastContactedAt",
                table: "customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "customers",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_ProjectId",
                table: "customers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_TeamId_ProjectId",
                table: "customers",
                columns: new[] { "TeamId", "ProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_customers_projects_ProjectId",
                table: "customers",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customers_projects_ProjectId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_ProjectId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_TeamId_ProjectId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "FollowUpStatus",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "LastContactedAt",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "customers");
        }
    }
}
