using Microsoft.Extensions.DependencyInjection;
using Funda.Common.BackgroundProcessing;

namespace Funda.Common.Warmup;

public static class WarmupProcessorsRegistrationExtensions
{
    public static IServiceCollection AddWarmup(this IServiceCollection services, Action<IWarmupConfigurator> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var configurator = new WarmupConfigurator();
        configure(configurator);

        services.AddSingleton<WarmupCoordinator>(sp =>
        {
            var initState = sp.GetRequiredService<IInitializationState>();
            return new WarmupCoordinator(initState, configurator.RequiredProcessors);
        });
        return services;
    }
}
