using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBehaviorExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserBehaviors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_ExpiresAt",
                table: "UserBehaviors",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_ExpiresAt",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserBehaviors");
        }
    }
}
