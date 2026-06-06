using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class RoleAccessTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public RoleAccessTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Anonymous_user_cannot_access_admin_or_seller_routes()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var client = factory.CreateClient();

        (await client.GetAsync("/api/admin/dashboard/overview")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync("/api/seller/dashboard/overview")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customer_role_cannot_access_admin_or_seller_routes()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "customer-role-1",
            email: "customer-role@test.local",
            fullName: "Customer Role",
            role: "Customer");

        (await client.GetAsync("/api/admin/dashboard/overview")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync("/api/seller/dashboard/overview")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Seller_claim_without_seller_db_role_is_rejected_by_seller_handler()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == "role-mismatch-1");
            if (user is null)
            {
                user = new User
                {
                    KeycloakId = "role-mismatch-1",
                    Email = "role-mismatch@test.local",
                    FullName = "Role Mismatch",
                    Role = UserRole.Customer,
                    Status = UserStatus.Active
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }

        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "role-mismatch-1",
            email: "role-mismatch@test.local",
            fullName: "Role Mismatch",
            role: "Seller");

        var resp = await client.GetAsync("/api/seller/dashboard/overview");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
