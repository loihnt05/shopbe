using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shopbe.Application;
using Shopbe.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var keycloakAuthority = builder.Configuration["Authentication:Keycloak:Authority"];

if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Missing Authentication:Keycloak:Authority configuration.");
}

// Add services to the container.
builder.Services.AddControllers();
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

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a Keycloak access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
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

