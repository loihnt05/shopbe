using System.Security.Claims;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Web.Common;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? KeycloakId => Principal?.FindFirstValue("sub");
    public string? Email => Principal?.FindFirstValue("email");

    public string? PreferredUsername => Principal?.FindFirstValue("preferred_username");

    public string? FullName
    {
        get
        {
            var principal = Principal;
            if (principal is null) return null;

            // Keycloak commonly populates `name`, or `given_name` + `family_name`.
            var name = principal.FindFirstValue("name");
            if (!string.IsNullOrWhiteSpace(name)) return name;

            var given = principal.FindFirstValue("given_name");
            var family = principal.FindFirstValue("family_name");
            var combined = $"{given} {family}".Trim();
            if (!string.IsNullOrWhiteSpace(combined)) return combined;

            // Fallback to preferred username as a last resort (better than empty).
            return PreferredUsername;
        }
    }
}

