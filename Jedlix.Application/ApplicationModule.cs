using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Jedlix.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        return services;
    }
}   