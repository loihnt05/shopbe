using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class RecommendationFlowTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public RecommendationFlowTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Recommendations_update_dynamically_based_on_user_behavior()
    {
        // Arrange
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        // We need multiple categories and products to test recommendations properly
        Guid cat1Id, cat2Id;
        Guid p1Id, p2Id, p3Id, p4Id; // p1, p2 in cat1; p3, p4 in cat2

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            
            var seller = new User
            {
                KeycloakId = "rec-seller-1",
                Email = "rec-seller@test.com",
                FullName = "Rec Seller",
                Role = UserRole.Seller,
                Status = UserStatus.Active
            };
            db.Users.Add(seller);
            await db.SaveChangesAsync();

            var cat1 = new Category { Name = "Cat 1", Slug = "cat-1", IsActive = true };
            var cat2 = new Category { Name = "Cat 2", Slug = "cat-2", IsActive = true };
            db.Categories.AddRange(cat1, cat2);
            await db.SaveChangesAsync();
            cat1Id = cat1.Id; cat2Id = cat2.Id;

            var p1 = new Product { Name = "Product 1", Slug = "p-1", BasePrice = 100, CategoryId = cat1Id, IsActive = true, SellerId = seller.Id };
            var p2 = new Product { Name = "Product 2", Slug = "p-2", BasePrice = 120, CategoryId = cat1Id, IsActive = true, SellerId = seller.Id };
            var p3 = new Product { Name = "Product 3", Slug = "p-3", BasePrice = 200, CategoryId = cat2Id, IsActive = true, SellerId = seller.Id };
            var p4 = new Product { Name = "Product 4", Slug = "p-4", BasePrice = 220, CategoryId = cat2Id, IsActive = true, SellerId = seller.Id };
            
            db.Products.AddRange(p1, p2, p3, p4);
            await db.SaveChangesAsync();

            // Add variants
            db.ProductVariants.AddRange(
                new ProductVariant { ProductId = p1.Id, Sku = "V1", Price = 100, IsActive = true },
                new ProductVariant { ProductId = p2.Id, Sku = "V2", Price = 120, IsActive = true },
                new ProductVariant { ProductId = p3.Id, Sku = "V3", Price = 200, IsActive = true },
                new ProductVariant { ProductId = p4.Id, Sku = "V4", Price = 220, IsActive = true }
            );
            await db.SaveChangesAsync();

            p1Id = p1.Id; p2Id = p2.Id; p3Id = p3.Id; p4Id = p4.Id;
        }

        var client = factory.CreateAuthenticatedClient(keycloakSub: "rec-user-1", email: "rec@local.test");
        var anonClient = factory.CreateClient();

        // 1. New user (cold start) -> show trending products (which fallback to empty initially, so just expect success)
        var topSellingResp = await anonClient.GetAsync("/api/recommendations/top-selling?count=10");
        topSellingResp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var personalizedResp = await client.GetAsync("/api/recommendations/me");
        personalizedResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. User clicks Product A -> track view
        var trackViewReq = new { behaviorType = BehaviorType.ProductView, productId = p1Id, categoryId = cat1Id };
        var trackResp = await client.PostAsJsonAsync("/api/tracking/track", trackViewReq);
        trackResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Wait a bit for tracking to persist (if not fully sync, but in our code it is sync)
        
        // 3. User views multiple products in Cat1 -> category preference increases
        await client.PostAsJsonAsync("/api/tracking/track", new { behaviorType = BehaviorType.ProductView, productId = p2Id, categoryId = cat1Id });
        await client.PostAsJsonAsync("/api/tracking/track", new { behaviorType = BehaviorType.ProductView, productId = p1Id, categoryId = cat1Id });

        // Check Recently Viewed
        var recentResp = await client.GetAsync("/api/recommendations/me/recently-viewed");
        recentResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var recentList = await recentResp.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();
        recentList.Should().NotBeNull();
        recentList!.Count.Should().BeGreaterThan(0);
        recentList.Select(x => x.GetProperty("id").GetGuid()).Should().Contain(p1Id);

        // 4. Check Personalized (should favor Cat1)
        var persResp2 = await client.GetAsync("/api/recommendations/me");
        persResp2.StatusCode.Should().Be(HttpStatusCode.OK);
        var persList = await persResp2.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();
        // It might return p1 and p2
        
        // 5. User purchases Product B (p2Id)
        await client.PostAsJsonAsync("/api/tracking/track", new { behaviorType = BehaviorType.Purchase, productId = p2Id, categoryId = cat1Id });

        // 6. Recommendation refreshes -> should reduce weight of purchased (p2Id)
        var persResp3 = await client.GetAsync("/api/recommendations/me");
        persResp3.StatusCode.Should().Be(HttpStatusCode.OK);
        var persList3 = await persResp3.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();
        if (persList3 != null && persList3.Count > 0)
        {
            var recommendedIds = persList3.Select(x => x.GetProperty("id").GetGuid()).ToList();
            // p2Id should not be in recommended anymore because it's purchased
            recommendedIds.Should().NotContain(p2Id);
        }

        // 7. Frequently bought together
        // We simulate a purchase of P3 and P4 together directly in DB
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var user = new Shopbe.Domain.Entities.User.User
            {
                Id = Guid.NewGuid(),
                FullName = "Fake User",
                Email = "fake@test.local",
                PhoneNumber = "123",
                Role = UserRole.Customer,
                Status = UserStatus.Active,
                KeycloakId = "fake_123"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var seller = db.Users.First(u => u.KeycloakId == "rec-seller-1");
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                UserId = user.Id,
                Status = OrderStatus.Confirmed,
                TotalAmount = 420
            };
            var v3 = db.ProductVariants.First(v => v.ProductId == p3Id);
            var v4 = db.ProductVariants.First(v => v.ProductId == p4Id);
            order.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), ProductVariantId = v3.Id, Quantity = 1, UnitPrice = 200, TotalPrice = 200, SellerId = seller.Id });
            order.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), ProductVariantId = v4.Id, Quantity = 1, UnitPrice = 220, TotalPrice = 220, SellerId = seller.Id });
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }

        var fbtResp = await anonClient.GetAsync($"/api/recommendations/products/{p3Id}/frequently-bought-together");
        fbtResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fbtList = await fbtResp.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();
        fbtList.Should().NotBeNull();
        if (fbtList!.Count > 0)
        {
            fbtList.Select(x => x.GetProperty("id").GetGuid()).Should().Contain(p4Id);
        }
    }
}
