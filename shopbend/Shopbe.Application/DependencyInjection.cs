using Microsoft.Extensions.DependencyInjection;

namespace Shopbe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register AutoMapper profiles from this assembly
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
}