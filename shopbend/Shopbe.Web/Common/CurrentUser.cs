using System.Security.Claims;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Web.Common;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? KeycloakId
    {
        get
        {
            var principal = Principal;
            if (principal is null) return null;

            // Standard OIDC subject
            var sub = principal.FindFirstValue("sub");
            if (!string.IsNullOrWhiteSpace(sub)) return sub;

            // Some token handlers map subject to NameIdentifier
            var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(nameIdentifier)) return nameIdentifier;

            // Fallback: some providers use this URI
            var oid = principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (!string.IsNullOrWhiteSpace(oid)) return oid;

            return null;
        }
    }
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

