using Microsoft.Extensions.DependencyInjection;

namespace da.Infrastructure.DependencyInjections;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services) => services;
}