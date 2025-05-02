using Microsoft.Extensions.DependencyInjection;

namespace Da.Application.DependencyInjections;

public static class ServiceContainer
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services;
}