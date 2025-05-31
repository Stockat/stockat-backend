using Microsoft.Extensions.DependencyInjection;
using Stockat.Core.IServices;
using Stockat.Service.Services;

namespace Stockat.Service;

public static class ModuleServiceDependencies
{
    public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
    {
        services.AddTransient<ILoggerManager, LoggerManager>();
        
        return services;
    }
}
