using Microsoft.Extensions.DependencyInjection;

namespace Da.Infrastructure.DependencyInjections;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services) => services;
}