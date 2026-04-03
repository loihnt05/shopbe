namespace Shopbe.Application.Common.Interfaces;

public interface ICurrentUser
{
    /// <summary>
    /// Keycloak subject (sub claim). This is the canonical identifier for syncing user accounts.
    /// </summary>
    string? KeycloakId { get; }

    string? Email { get; }
    string? FullName { get; }
    string? PreferredUsername { get; }
}

