using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shopbe.E2E.Tests.Infrastructure;

/// <summary>
/// Authentication handler for E2E tests.
/// Reads identity from request headers so tests can control user.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Sub", out var subHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var sub = subHeader.ToString();
        var email = Request.Headers.TryGetValue("X-Test-Email", out var emailHeader)
            ? emailHeader.ToString()
            : "e2e@local.test";
        var name = Request.Headers.TryGetValue("X-Test-Name", out var nameHeader)
            ? nameHeader.ToString()
            : null;
        var role = Request.Headers.TryGetValue("X-Test-Role", out var roleHeader)
            ? roleHeader.ToString()
            : "Customer";

        var claims = new List<Claim>
        {
            new("sub", sub),
            new("preferred_username", "e2e-user"),
            new("email", email),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim("name", name));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}


