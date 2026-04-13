using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shopbe.Application;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Infrastructure;
using Shopbe.Infrastructure.Persistence;
using Shopbe.Web.Common;

var builder = WebApplication.CreateBuilder(args);
var keycloakAuthority = builder.Configuration["Authentication:Keycloak:Authority"];
var keycloakAuthorityExternal = builder.Configuration["Authentication:Keycloak:AuthorityExternal"];

if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Missing Authentication:Keycloak:Authority configuration.");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Authentication:Keycloak:RequireHttpsMetadata", 
                                        !builder.Environment.IsDevelopment());

        var validAudiences = builder.Configuration
            .GetSection("Authentication:Keycloak:ValidAudiences")
            .Get<string[]>();

        // Keycloak can issue tokens with an issuer (iss) matching the URL used by the browser
        // (e.g. http://localhost:8080/...) while the API container validates metadata using an
        // internal docker hostname (e.g. http://keycloak:8080/...). Accept both.
        var validIssuers = new[] { keycloakAuthority, keycloakAuthorityExternal }
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = validIssuers,
            ValidateAudience = builder.Configuration.GetValue("Authentication:Keycloak:ValidateAudience", false),
            ValidAudiences = validAudiences,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                {
                    return Task.CompletedTask;
                }

                AddRoleClaims(identity, context.Principal.FindFirst("realm_access")?.Value, null);

                var resourceAccessClaims = context.Principal
                    .FindAll("resource_access")
                    .Select(claim => claim.Value)
                    .ToList();

                foreach (var resourceAccessClaim in resourceAccessClaims)
                {
                    AddRoleClaims(identity, resourceAccessClaim, "resource");
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {   
        Title = "Shopbe API",
        Version = "v1"
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{(string.IsNullOrWhiteSpace(keycloakAuthorityExternal) ? keycloakAuthority : keycloakAuthorityExternal)}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{(string.IsNullOrWhiteSpace(keycloakAuthorityExternal) ? keycloakAuthority : keycloakAuthorityExternal)}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "Profile" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile" }
        }
    });
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply EF Core migrations automatically (useful for local/dev & Docker compose)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.Migrate();

    // Seed sample data in Development for quick manual testing.
    if (app.Environment.IsDevelopment())
    {
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("ShopbeDbSeeder");
        await ShopbeDbSeeder.SeedAsync(db, logger);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["Authentication:Keycloak:SwaggerClientId"] ?? "shopbe-swagger");
        options.OAuthScopes("openid", "profile");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();

// Automatically sync Keycloak user (sub/email/name claims) into the application database.
app.UseMiddleware<UserSyncMiddleware>();

app.UseAuthorization();

app.MapControllers();


app.Run();

static void AddRoleClaims(ClaimsIdentity identity, string? json, string? resourcePrefix)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return;
    }

    using var document = JsonDocument.Parse(json);
    var root = document.RootElement;

    if (resourcePrefix is null)
    {
        if (root.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in rolesElement.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value) && !identity.HasClaim(ClaimTypes.Role, value))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, value));
                }
            }
        }

        return;
    }

    foreach (var resource in root.EnumerateObject())
    {
        if (!resource.Value.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
        {
            continue;
        }

        foreach (var role in rolesElement.EnumerateArray())
        {
            var value = role.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var claimValue = $"{resource.Name}:{value}";
            if (!identity.HasClaim(ClaimTypes.Role, claimValue))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, claimValue));
            }
        }
    }
}

// Expose Program for integration/E2E tests (WebApplicationFactory<Program>).
public partial class Program;

