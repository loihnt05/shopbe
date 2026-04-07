using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Shopbe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register AutoMapper with profiles from this assembly
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}