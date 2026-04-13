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
        var sub = Request.Headers.TryGetValue("X-Test-Sub", out var subHeader)
            ? subHeader.ToString()
            : "e2e-sub-1";

        var email = Request.Headers.TryGetValue("X-Test-Email", out var emailHeader)
            ? emailHeader.ToString()
            : "e2e@local.test";

        var claims = new List<Claim>
        {
            new("sub", sub),
            new("preferred_username", "e2e-user"),
            new("email", email),
            new(ClaimTypes.Role, "Customer")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}



