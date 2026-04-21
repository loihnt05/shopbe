using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbe.Infrastructure.Persistence.Migrations;

public partial class AddReviewUniqueness : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Reviews_OrderId_ProductId_UserId",
            table: "Reviews",
            columns: new[] { "OrderId", "ProductId", "UserId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Reviews_OrderId_ProductId_UserId",
            table: "Reviews");
    }
}

