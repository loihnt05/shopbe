using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "PendingVerification",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "PendingVerification");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Customer",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Customer");

            migrationBuilder.CreateTable(
                name: "ShippingZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingZoneDistricts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    District = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingZoneDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingZoneDistricts_ShippingZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "ShippingZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZoneDistricts_City_District",
                table: "ShippingZoneDistricts",
                columns: new[] { "City", "District" });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZoneDistricts_ZoneId",
                table: "ShippingZoneDistricts",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZoneDistricts_ZoneId_City_District",
                table: "ShippingZoneDistricts",
                columns: new[] { "ZoneId", "City", "District" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingZones_Name",
                table: "ShippingZones",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingZoneDistricts");

            migrationBuilder.DropTable(
                name: "ShippingZones");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PendingVerification",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "PendingVerification");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Customer",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "Customer");
        }
    }
}
