using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerAndRBAC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShopDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShopLogoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ShopBannerUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CommissionRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.05m),
                    Rating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    TotalSales = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ApprovalStatus",
                table: "Products",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SellerId",
                table: "OrderItems",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO ""Users"" (""Id"", ""KeycloakId"", ""Email"", ""FullName"", ""Status"", ""Role"", ""CreatedAt"", ""UpdatedAt"")
SELECT '11111111-1111-1111-1111-111111111111',
       'migration-admin-keycloak-id',
       'migration-admin@shopbee.local',
       'Migration Admin',
       'Active',
       'Admin',
       CURRENT_TIMESTAMP,
       CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Users""
    WHERE ""Role"" = 'Admin'
);
");

            migrationBuilder.Sql(@"
UPDATE ""Products"" AS p
SET ""SellerId"" = admin.""Id""
FROM (
    SELECT ""Id""
    FROM ""Users""
    WHERE ""Role"" = 'Admin'
    ORDER BY ""CreatedAt""
    LIMIT 1
) AS admin
WHERE p.""SellerId"" = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE ""OrderItems"" AS oi
SET ""SellerId"" = COALESCE(NULLIF(p.""SellerId"", '00000000-0000-0000-0000-000000000000'), admin.""Id"")
FROM ""ProductVariants"" AS pv
JOIN ""Products"" AS p ON p.""Id"" = pv.""ProductId""
LEFT JOIN (
    SELECT ""Id""
    FROM ""Users""
    WHERE ""Role"" = 'Admin'
    ORDER BY ""CreatedAt""
    LIMIT 1
) AS admin ON TRUE
WHERE oi.""ProductVariantId"" = pv.""Id""
  AND oi.""SellerId"" = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE ""OrderItems""
SET ""SellerId"" = admin.""Id""
FROM (
    SELECT ""Id""
    FROM ""Users""
    WHERE ""Role"" = 'Admin'
    ORDER BY ""CreatedAt""
    LIMIT 1
) AS admin
WHERE ""OrderItems"".""SellerId"" = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM ""Products""
        WHERE ""SellerId"" = '00000000-0000-0000-0000-000000000000'
    ) THEN
        RAISE EXCEPTION 'Cannot apply RBAC migration: some Products rows still have empty SellerId.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM ""OrderItems""
        WHERE ""SellerId"" = '00000000-0000-0000-0000-000000000000'
    ) THEN
        RAISE EXCEPTION 'Cannot apply RBAC migration: some OrderItems rows still have empty SellerId.';
    END IF;
END $$;
");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Users_SellerId",
                table: "OrderItems",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Users_SellerId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "SellerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Products_ApprovalStatus",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SellerId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "OrderItems");
        }
    }
}
