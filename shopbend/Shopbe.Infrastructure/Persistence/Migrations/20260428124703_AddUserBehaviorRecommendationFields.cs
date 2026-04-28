using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBehaviorRecommendationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "UserBehaviors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "UserBehaviors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "UserBehaviors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserBehaviors",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "UserBehaviors",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "UserBehaviors",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "UserBehaviors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "UserBehaviors",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_CategoryId",
                table: "UserBehaviors",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_CorrelationId",
                table: "UserBehaviors",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_OrderId",
                table: "UserBehaviors",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_UserId_CategoryId_BehaviorType",
                table: "UserBehaviors",
                columns: new[] { "UserId", "CategoryId", "BehaviorType" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Categories_CategoryId",
                table: "UserBehaviors",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBehaviors_Orders_OrderId",
                table: "UserBehaviors",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Categories_CategoryId",
                table: "UserBehaviors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBehaviors_Orders_OrderId",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_CategoryId",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_CorrelationId",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_OrderId",
                table: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_UserBehaviors_UserId_CategoryId_BehaviorType",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "City",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "UserBehaviors");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "UserBehaviors");
        }
    }
}
