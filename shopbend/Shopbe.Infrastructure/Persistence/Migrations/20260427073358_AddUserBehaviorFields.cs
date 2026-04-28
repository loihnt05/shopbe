using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBehaviorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Products_ProductId",
                table: "UserBehaviors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Users_UserId",
                table: "UserBehaviors");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "UserBehaviors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "UserBehaviors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "BehaviorType",
                table: "UserBehaviors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserBehaviors",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                table: "UserBehaviors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "UserBehaviors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referrer",
                table: "UserBehaviors",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserBehaviors",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "UserBehaviors",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    To = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: false),
                    BodyText = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_OccurredAt",
                table: "UserBehaviors",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_ProductId_OccurredAt",
                table: "UserBehaviors",
                columns: new[] { "ProductId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_SessionId",
                table: "UserBehaviors",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_UserId_OccurredAt",
                table: "UserBehaviors",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_UserId_ProductId_BehaviorType",
                table: "UserBehaviors",
                columns: new[] { "UserId", "ProductId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrderId_ProductId_UserId",
                table: "Reviews",
                columns: new[] { "OrderId", "ProductId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_IdempotencyKey",
                table: "EmailMessages",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Products_ProductId",
                table: "UserBehaviors",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Users_UserId",
                table: "UserBehaviors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Products_ProductId",
                table: "UserBehaviors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Users_UserId",
                table: "UserBehaviors");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_OccurredAt",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_ProductId_OccurredAt",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_SessionId",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_UserId_OccurredAt",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_UserId_ProductId_BehaviorType",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderId_ProductId_UserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "BehaviorType",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Referrer",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "UserBehaviors");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "UserBehaviors",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "UserBehaviors",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Products_ProductId",
                table: "UserBehaviors",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Users_UserId",
                table: "UserBehaviors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
