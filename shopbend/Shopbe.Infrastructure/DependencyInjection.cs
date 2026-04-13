using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Infrastructure.Persistence;
using Shopbe.Infrastructure.Repositories;
using Stripe;

namespace Shopbe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.Configure<StripeOptions>(opts => configuration.GetSection("Stripe").Bind(opts));
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        services.AddScoped<IStripeService, StripeService>();
        
        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        

        return services;
    }
}