using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Infrastructure.Persistence;
using Shopbe.Infrastructure.Repositories;
using Shopbe.Infrastructure.Services;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Infrastructure.Services.Email;
using Shopbe.Infrastructure.Storage;
using Stripe;

namespace Shopbe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Redis (IDistributedCache)
        // Prefer: Redis:ConnectionString, fallback: ConnectionStrings:Redis
        var redisConnectionString = configuration["Redis:ConnectionString"]
                                   ?? configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = configuration["Redis:InstanceName"] ?? "shopbe:";
            });

            // Application cache abstraction backed by Redis distributed cache
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        
        services.Configure<StripeOptions>(opts => configuration.GetSection("Stripe").Bind(opts));
        services.AddSingleton<IConfigureOptions<StripeOptions>, ConfigureStripeConfiguration>();
        services.AddScoped<IStripeService, StripeService>();
        
        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Local uploads storage (review images, etc.)
        services.AddScoped<IFileStorage, LocalFileStorage>();

        // Email notifications
        services.Configure<SmtpEmailOptions>(opts => configuration.GetSection("Email:Smtp").Bind(opts));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailQueue, HangfireEmailQueue>();
        

        return services;
    }
}

internal sealed class ConfigureStripeConfiguration : IConfigureOptions<StripeOptions>
{
    public void Configure(StripeOptions options)
    {
        // Stripe SDK stores the API key in a static global; set it once once options are bound.
        StripeConfiguration.ApiKey = options.SecretKey;
    }
}
