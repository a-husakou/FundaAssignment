using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Funda.Common.BackgroundProcessing;

public static class BackgroundProcessingRegistrationExtensions
{
    public static IServiceCollection AddBackgroundProcessor<TProcessor>(
        this IServiceCollection services,
        TimeSpan interval)
        where TProcessor : class, IBackgroundProcessor
    {
        services.AddTransient<TProcessor>();
        services.AddSingleton<IHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>>>();
            return new ScheduledBackgroundProcessorHostedService<TProcessor>(sp, interval, logger);
        });

        return services;
    }

    public static IServiceCollection AddBackgroundProcessor<TProcessor>(
        this IServiceCollection services,
        IConfiguration configurationSection)
        where TProcessor : class, IBackgroundProcessor
    {
        var intervalString = configurationSection["Interval"]; // expected format: HH:mm:ss
        if (string.IsNullOrWhiteSpace(intervalString))
        {
            throw new InvalidOperationException($"Missing required configuration 'Interval' for background processor '{typeof(TProcessor).Name}'.");
        }
        if (!TimeSpan.TryParse(intervalString, out var interval))
        {
            throw new InvalidOperationException($"Invalid TimeSpan format for 'Interval' in background processor '{typeof(TProcessor).Name}': '{intervalString}'.");
        }

        return services.AddBackgroundProcessor<TProcessor>(interval);
    }
}
