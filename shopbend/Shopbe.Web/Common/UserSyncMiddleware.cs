using Microsoft.EntityFrameworkCore;
using Shopbe.Infrastructure.Persistence;
using DomainUser = Shopbe.Domain.Entities.User.User;

namespace Shopbe.Web.Common;

/// <summary>
/// Ensures an authenticated Keycloak user (by `sub` claim) exists in the application database.
/// 
/// - Idempotent: will only insert when missing.
/// - Updates basic profile fields when present in token claims.
/// - Skips for anonymous requests.
/// </summary>
public sealed class UserSyncMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ShopDbContext db)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // Keycloak canonical identifier
        var keycloakId = user.FindFirst("sub")?.Value
                         ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            await next(context);
            return;
        }

        var email = user.FindFirst("email")?.Value;
        var preferredUsername = user.FindFirst("preferred_username")?.Value;
        var fullName = user.FindFirst("name")?.Value;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var given = user.FindFirst("given_name")?.Value;
            var family = user.FindFirst("family_name")?.Value;
            fullName = $"{given} {family}".Trim();
        }

        // Only create/update when needed.
        var dbUser = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        if (dbUser is null)
        {
            // Create minimal record; avoid linking by email automatically.
            dbUser = new DomainUser
            {
                KeycloakId = keycloakId,
                Email = email ?? string.Empty,
                FullName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName
                    : (!string.IsNullOrWhiteSpace(preferredUsername) ? preferredUsername : keycloakId),
            };

            db.Users.Add(dbUser);
            await db.SaveChangesAsync();
        }
        else
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(email) && !string.Equals(dbUser.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                dbUser.Email = email;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(fullName) && !string.Equals(dbUser.FullName, fullName, StringComparison.Ordinal))
            {
                dbUser.FullName = fullName;
                changed = true;
            }

            // Optional: if you store username, map it here.

            if (changed)
            {
                await db.SaveChangesAsync();
            }
        }

        await next(context);
    }
}


