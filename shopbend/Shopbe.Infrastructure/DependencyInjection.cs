using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}