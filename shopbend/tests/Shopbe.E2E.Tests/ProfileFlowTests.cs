using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class ProfileFlowTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public ProfileFlowTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task User_can_sync_profile_and_retrieve_it()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var client = factory.CreateAuthenticatedClient(keycloakSub: "profile-e2e-1", email: "profile@local.test");

        // Act 1: Sync profile for the first time
        var syncPayload = new
        {
            fullName = "E2E User",
            email = "profile@local.test",
            phoneNumber = "0912345678",
            gender = "male",
            birthday = "1990-06-15T00:00:00Z",
            language = "English (US)",
            country = "Vietnam",
            avatarUrl = ""
        };

        var syncResp = await client.PostAsJsonAsync("/api/users/sync", syncPayload);
        syncResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var syncResult = await syncResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        syncResult.GetProperty("keycloakId").GetString().Should().Be("profile-e2e-1");
        syncResult.GetProperty("email").GetString().Should().Be("profile@local.test");
        syncResult.GetProperty("fullName").GetString().Should().Be("E2E User");
        syncResult.GetProperty("phoneNumber").GetString().Should().Be("0912345678");
        syncResult.GetProperty("gender").GetString().Should().Be("male");
        syncResult.GetProperty("language").GetString().Should().Be("English (US)");
        syncResult.GetProperty("country").GetString().Should().Be("Vietnam");

        // Act 2: Retrieve profile by keycloak
        var getResp = await client.GetAsync("/api/users/by-keycloak");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResult = await getResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        getResult.GetProperty("keycloakId").GetString().Should().Be("profile-e2e-1");
        getResult.GetProperty("fullName").GetString().Should().Be("E2E User");
        getResult.GetProperty("phoneNumber").GetString().Should().Be("0912345678");

        // Act 3: Update profile via sync (idempotent)
        var updatePayload = new
        {
            fullName = "E2E User Updated",
            email = "profile@local.test",
            phoneNumber = "0999999999",
            gender = "male",
            birthday = "1990-06-15T00:00:00Z",
            language = "Vietnamese",
            country = "Vietnam",
            avatarUrl = ""
        };

        var updateResp = await client.PostAsJsonAsync("/api/users/sync", updatePayload);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResult = await updateResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        updateResult.GetProperty("fullName").GetString().Should().Be("E2E User Updated");
        updateResult.GetProperty("phoneNumber").GetString().Should().Be("0999999999");
        updateResult.GetProperty("language").GetString().Should().Be("Vietnamese");

        // Verify in DB
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            var user = await db.Users.AsNoTracking().FirstAsync(u => u.KeycloakId == "profile-e2e-1");
            user.FullName.Should().Be("E2E User Updated");
            user.PhoneNumber.Should().Be("0999999999");
            user.Language.Should().Be("Vietnamese");
        }

        // Act 4: Second sync should still work (idempotent)
        var secondSyncResp = await client.PostAsJsonAsync("/api/users/sync", syncPayload);
        secondSyncResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Sync_creates_different_profiles_for_different_users()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        var client1 = factory.CreateAuthenticatedClient(keycloakSub: "profile-e2e-a", email: "a@local.test");
        var client2 = factory.CreateAuthenticatedClient(keycloakSub: "profile-e2e-b", email: "b@local.test");

        var payload1 = new { fullName = "User A", email = "a@local.test", phoneNumber = "", gender = "male", birthday = (string?)null, language = "English (US)", country = "Vietnam", avatarUrl = "" };
        var payload2 = new { fullName = "User B", email = "b@local.test", phoneNumber = "", gender = "female", birthday = (string?)null, language = "English (US)", country = "Vietnam", avatarUrl = "" };

        var resp1 = await client1.PostAsJsonAsync("/api/users/sync", payload1);
        var resp2 = await client2.PostAsJsonAsync("/api/users/sync", payload2);

        resp1.StatusCode.Should().Be(HttpStatusCode.OK);
        resp2.StatusCode.Should().Be(HttpStatusCode.OK);

        var user1 = await client1.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/users/by-keycloak");
        var user2 = await client2.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/users/by-keycloak");

        user1.GetProperty("fullName").GetString().Should().Be("User A");
        user2.GetProperty("fullName").GetString().Should().Be("User B");
        user1.GetProperty("id").GetString().Should().NotBe(user2.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Sync_preserves_claims_from_token_over_request()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        // email in token is "override@local.test" but request says "request@local.test"
        var client = factory.CreateAuthenticatedClient(keycloakSub: "profile-claims", email: "override@local.test");

        var payload = new { fullName = "Claims Test", email = "request@local.test", phoneNumber = "", gender = "other", birthday = (string?)null, language = "English (US)", country = "Vietnam", avatarUrl = "" };

        var resp = await client.PostAsJsonAsync("/api/users/sync", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        // token email should take precedence over request email
        result.GetProperty("email").GetString().Should().Be("override@local.test");
    }

    [Fact]
    public async Task Get_by_keycloak_returns_401_when_not_authenticated()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var anonymousClient = factory.CreateClient();

        var resp = await anonymousClient.GetAsync("/api/users/by-keycloak");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Sync_returns_401_when_not_authenticated()
    {
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);
        var anonymousClient = factory.CreateClient();

        var payload = new { fullName = "Hacker", email = "hacker@evil.com", phoneNumber = "", gender = "male", birthday = (string?)null, language = "English (US)", country = "US", avatarUrl = "" };

        var resp = await anonymousClient.PostAsJsonAsync("/api/users/sync", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
