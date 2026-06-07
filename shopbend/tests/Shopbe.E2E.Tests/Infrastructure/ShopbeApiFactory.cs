using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shopbe.Application.Common.Interfaces;
using Shopbe.E2E.Tests.Fakes;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests.Infrastructure;

/// <summary>
/// Boots the real API in-memory using TestServer but swaps infrastructure pieces for E2E tests.
/// </summary>
public sealed class ShopbeApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ShopbeApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Provide minimal config, so Program.cs doesn't throw on missing Keycloak authority.
        builder.UseSetting("Authentication:Keycloak:Authority", "http://test-keycloak");
        builder.UseSetting("Authentication:Keycloak:AuthorityExternal", "http://test-keycloak");
        builder.UseSetting("Authentication:Keycloak:RequireHttpsMetadata", "false");
        builder.UseSetting("Authentication:Keycloak:ValidateAudience", "false");
        builder.UseSetting("HttpsRedirection:Enabled", "false");

        builder.ConfigureServices(services =>
        {
            services.AddLogging(logging =>
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning);
                logging.AddFilter("Hangfire", LogLevel.Warning);
                logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);
                logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
            });

            // Replace DbContext with a real Postgres connection from Testcontainers.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ShopDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ShopDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            // Swap Stripe with deterministic fake.
            var stripeDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStripeService));
            if (stripeDescriptor is not null)
            {
                services.Remove(stripeDescriptor);
            }

            services.AddScoped<IStripeService, FakeStripeService>();

            // Replace authentication with a test scheme that always signs in a known user.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            services.AddAuthorization();

            // Ensure DB is migrated for each test run.
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            db.Database.Migrate();
        });
    }

    public HttpClient CreateAuthenticatedClient(string keycloakSub = "e2e-sub-1", string? email = "e2e@local.test", string? fullName = null, string role = "Customer")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Sub", keycloakSub);
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        if (!string.IsNullOrWhiteSpace(email))
        {
            client.DefaultRequestHeaders.Add("X-Test-Email", email);
        }
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            client.DefaultRequestHeaders.Add("X-Test-Name", fullName);
        }

        return client;
    }
}
