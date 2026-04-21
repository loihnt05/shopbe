using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Infrastructure.Persistence;
using Shopbe.Infrastructure.Repositories;
using Shopbe.Infrastructure.Storage;
using Stripe;

namespace Shopbe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.Configure<StripeOptions>(opts => configuration.GetSection("Stripe").Bind(opts));
        services.AddSingleton<IConfigureOptions<StripeOptions>, ConfigureStripeConfiguration>();
        services.AddScoped<IStripeService, StripeService>();
        
        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Local uploads storage (review images, etc.)
        services.AddScoped<IFileStorage, LocalFileStorage>();
        

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
