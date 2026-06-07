using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class DemoFlowVerificationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public DemoFlowVerificationTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Authenticated_request_links_existing_app_user_by_email()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "phase2-link@test.local");
            if (existingUser is null)
            {
                existingUser = new User
                {
                    KeycloakId = string.Empty,
                    Email = "phase2-link@test.local",
                    FullName = "Phase 2 Link User",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active
                };
                db.Users.Add(existingUser);
            }
            else
            {
                existingUser.KeycloakId = string.Empty;
                existingUser.Role = UserRole.Customer;
                existingUser.Status = UserStatus.Active;
            }

            await db.SaveChangesAsync();
        }

        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "phase2-linked-sub",
            email: "phase2-link@test.local",
            fullName: "Phase 2 Link User",
            role: "Customer");

        var meResp = await client.GetAsync("/api/auth/me");
        meResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var byKeycloakResp = await client.GetAsync("/api/users/by-keycloak");
        byKeycloakResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var users = await db.Users.Where(u => u.Email == "phase2-link@test.local").ToListAsync();
            users.Should().HaveCount(1);
            users[0].KeycloakId.Should().Be("phase2-linked-sub");
        }
    }

    [Fact]
    public async Task Admin_dashboard_overview_reflects_seeded_demo_order_data()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await DatabaseSeed.SeedDemoDashboardScenarioAsync(db);
        }

        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "phase2-admin",
            email: "phase2-admin@test.local",
            fullName: "Phase 2 Admin",
            role: "Admin");

        var response = await client.GetAsync("/api/admin/dashboard/overview");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        json.GetProperty("totalSellers").GetInt32().Should().BeGreaterOrEqualTo(2);
        json.GetProperty("totalOrders").GetInt32().Should().BeGreaterOrEqualTo(2);
        json.GetProperty("pendingOrders").GetInt32().Should().BeGreaterOrEqualTo(1);
        json.GetProperty("completedOrders").GetInt32().Should().BeGreaterOrEqualTo(1);
        json.GetProperty("totalRevenue").GetDecimal().Should().BeGreaterOrEqualTo(120_000m);
        json.GetProperty("recentOrders").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Seller_dashboard_and_order_list_reflect_seller_owned_demo_data()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        DatabaseSeed.DemoDashboardSeedResult seed;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            seed = await DatabaseSeed.SeedDemoDashboardScenarioAsync(db);
        }

        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "phase2-seller1",
            email: "phase2-seller1@test.local",
            fullName: "Phase 2 Seller One",
            role: "Seller");

        var dashboardResp = await client.GetAsync("/api/seller/dashboard/overview");
        dashboardResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboardJson = await dashboardResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        dashboardJson.GetProperty("deliveredOrders").GetInt32().Should().BeGreaterOrEqualTo(1);
        dashboardJson.GetProperty("pendingOrders").GetInt32().Should().Be(0);
        dashboardJson.GetProperty("totalRevenue").GetDecimal().Should().Be(seed.DeliveredOrderRevenue);

        var ordersResp = await client.GetAsync("/api/seller/orders");
        ordersResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordersJson = await ordersResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var items = ordersJson.GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThan(0);
        items.EnumerateArray().Any(item =>
            item.GetProperty("customerEmail").GetString() == seed.CustomerEmail
            && item.GetProperty("status").GetInt32() == (int)OrderStatus.Delivered).Should().BeTrue();
    }
}
