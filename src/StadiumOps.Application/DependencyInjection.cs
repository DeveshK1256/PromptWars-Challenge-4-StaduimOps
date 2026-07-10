using Microsoft.Extensions.DependencyInjection;
using StadiumOps.Application.AI;
using StadiumOps.Application.Abstractions;

namespace StadiumOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IAiOrchestrator, AiOrchestrator>();
        return services;
    }
}
