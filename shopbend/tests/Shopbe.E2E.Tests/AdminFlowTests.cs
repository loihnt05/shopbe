using System.Net;
using FluentAssertions;
using Shopbe.E2E.Tests.Infrastructure;

namespace Shopbe.E2E.Tests;

public sealed class AdminFlowTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public AdminFlowTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Admin_can_access_dashboard_and_users_endpoints()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var client = factory.CreateAuthenticatedClient(
            keycloakSub: "admin-flow-1",
            email: "admin-flow@test.local",
            fullName: "Admin Flow",
            role: "Admin");

        var dashboardResp = await client.GetAsync("/api/admin/dashboard/overview");
        dashboardResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var usersResp = await client.GetAsync("/api/admin/users");
        usersResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
