using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class SellerFlowTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public SellerFlowTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Seller_can_access_dashboard_update_profile_and_create_product()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        Guid categoryId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var seed = await DatabaseSeed.SeedMinimalCatalogAsync(db);
            categoryId = seed.CategoryId;

            var seller = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == "seller-flow-1");
            if (seller is null)
            {
                seller = new User
                {
                    KeycloakId = "seller-flow-1",
                    Email = "seller-flow@test.local",
                    FullName = "Seller Flow",
                    Role = UserRole.Seller,
                    Status = UserStatus.Active,
                    SellerProfile = new SellerProfile
                    {
                        ShopName = "Flow Shop",
                        Status = SellerStatus.Active,
                        CommissionRate = 0.05m
                    }
                };
                db.Users.Add(seller);
                await db.SaveChangesAsync();
            }
        }

        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "seller-flow-1",
            email: "seller-flow@test.local",
            fullName: "Seller Flow",
            role: "Seller");

        var dashboardResp = await client.GetAsync("/api/seller/dashboard/overview");
        dashboardResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateProfileResp = await client.PutAsJsonAsync("/api/seller/profile", new
        {
            shopName = "Updated Flow Shop",
            shopDescription = "Seller flow test shop",
            shopLogoUrl = "https://example.com/logo.png",
            shopBannerUrl = "https://example.com/banner.png",
            contactPhone = "0900000000",
            contactEmail = "shop@test.local",
            address = "123 Test Street",
            city = "Test City"
        });
        updateProfileResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getProfileResp = await client.GetAsync("/api/seller/profile");
        getProfileResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileJson = await getProfileResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        profileJson.GetProperty("shopName").GetString().Should().Be("Updated Flow Shop");

        var createProductResp = await client.PostAsJsonAsync("/api/seller/products", new
        {
            name = "Seller Flow Product",
            description = "Created by seller flow test",
            basePrice = 199.99m,
            categoryId,
            brandId = (Guid?)null,
            isActive = true,
            images = new[]
            {
                new { imageUrl = "https://example.com/product.png", isPrimary = true }
            },
            variants = new[]
            {
                new { sku = "SELLER-FLOW-001", price = 199.99m, stockQuantity = 8, isActive = true, attributeValueIds = (Guid[]?)null }
            }
        });
        createProductResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResp = await client.GetAsync("/api/seller/products");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var listJson = await listResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var items = listJson.GetProperty("items");
        items.EnumerateArray().Any(x => x.GetProperty("name").GetString() == "Seller Flow Product").Should().BeTrue();
    }
}
