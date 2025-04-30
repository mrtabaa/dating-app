using Microsoft.Extensions.DependencyInjection;

namespace da.Application.DependencyInjections;

public static class ServiceContainer
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services;
}