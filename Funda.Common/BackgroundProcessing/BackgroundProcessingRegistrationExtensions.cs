using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Funda.Common.BackgroundProcessing;

public static class BackgroundProcessingRegistrationExtensions
{
    public static IServiceCollection AddBackgroundProcessor<TProcessor>(
        this IServiceCollection services,
        TimeSpan interval,
        bool performInitializationRun,
        BackgroundRetryOptions? retryOptions = null)
        where TProcessor : class, IBackgroundProcessor
    {
        services.AddTransient<TProcessor>();
        services.TryAddSingleton<BackgroundInitializationCoordinator, BackgroundInitializationCoordinator>();
        services.TryAddSingleton<IInitializationState>(sp => sp.GetRequiredService<BackgroundInitializationCoordinator>());
        services.AddSingleton<IHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>>>();
            var initCoordinator = sp.GetRequiredService<BackgroundInitializationCoordinator>();
            return new ScheduledBackgroundProcessorHostedService<TProcessor>(sp, interval, performInitializationRun, initCoordinator, logger, retryOptions);
        });

        return services;
    }

    public static IServiceCollection AddBackgroundProcessor<TProcessor>(
        this IServiceCollection services,
        IConfiguration configurationSection)
        where TProcessor : class, IBackgroundProcessor
    {
        var intervalString = configurationSection["Interval"];
        if (string.IsNullOrWhiteSpace(intervalString))
        {
            throw new InvalidOperationException($"Missing required configuration 'Interval' for background processor '{typeof(TProcessor).Name}'.");
        }
        if (!TimeSpan.TryParse(intervalString, out var interval))
        {
            throw new InvalidOperationException($"Invalid TimeSpan format for 'Interval' in background processor '{typeof(TProcessor).Name}': '{intervalString}'.");
        }
        var performInitString = configurationSection["PerformInitializationRun"];
        if (string.IsNullOrWhiteSpace(performInitString))
        {
            throw new InvalidOperationException($"Missing required configuration 'PerformInitializationRun' for background processor '{typeof(TProcessor).Name}'.");
        }
        if (!bool.TryParse(performInitString, out var performInitializationRun))
        {
            throw new InvalidOperationException($"Invalid boolean format for 'PerformInitializationRun' in background processor '{typeof(TProcessor).Name}': '{performInitString}'.");
        }

        // Optional retry configuration via binding + attribute validation
        BackgroundRetryOptions? retry = null;
        var retrySection = configurationSection.GetSection("Retry");
        if (retrySection.Exists())
        {
            retry = retrySection.Get<BackgroundRetryOptions>();
            if (retry != null)
            {
                var ctx = new ValidationContext(retry);
                Validator.ValidateObject(retry, ctx, validateAllProperties: true);
            }
        }

        return services.AddBackgroundProcessor<TProcessor>(interval, performInitializationRun, retry);
    }
}
