using Microsoft.Extensions.DependencyInjection;

namespace LprSolver.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigureServices(this IServiceCollection services)
    {
        // Register your services here

        return services;
    }
}
