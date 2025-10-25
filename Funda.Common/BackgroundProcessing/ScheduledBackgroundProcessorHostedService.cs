using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Funda.Common.BackgroundProcessing;

public class ScheduledBackgroundProcessorHostedService<TProcessor> : BackgroundService
    where TProcessor : class, IBackgroundProcessor
{
    private readonly IServiceProvider serviceProvider;
    private readonly TimeSpan interval;
    private readonly ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger;

    public ScheduledBackgroundProcessorHostedService(
        IServiceProvider serviceProvider,
        TimeSpan interval,
        ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger)
    {
        this.serviceProvider = serviceProvider;
        this.interval = interval;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately, then on the configured interval.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<TProcessor>();
                await processor.Execute(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background processor {Processor} failed.", typeof(TProcessor).Name);
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

