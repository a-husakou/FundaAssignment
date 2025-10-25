using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Funda.Common.BackgroundProcessing;

public class ScheduledBackgroundProcessorHostedService<TProcessor> : BackgroundService
    where TProcessor : class, IBackgroundProcessor
{
    private readonly IServiceProvider serviceProvider;
    private readonly TimeSpan interval;
    private readonly bool performInitializationRun;
    private readonly BackgroundInitializationCoordinator initializationCoordinator;
    private readonly ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger;

    public ScheduledBackgroundProcessorHostedService(
        IServiceProvider serviceProvider,
        TimeSpan interval,
        bool performInitializationRun,
        BackgroundInitializationCoordinator initializationCoordinator,
        ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger)
    {
        this.serviceProvider = serviceProvider;
        this.interval = interval;
        this.performInitializationRun = performInitializationRun;
        this.initializationCoordinator = initializationCoordinator;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (performInitializationRun)
        {
            bool initSuccess = await ExecuteInternalAsync(stoppingToken);
            if (initSuccess)
            {
                initializationCoordinator.ReportInitialized(typeof(TProcessor));
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            
            await ExecuteInternalAsync(stoppingToken);
        }
    }

    private async Task<bool> ExecuteInternalAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<TProcessor>();
            await processor.Execute(stoppingToken);
            return true;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Run of background processor {Processor} failed.", typeof(TProcessor).Name);
            return false;
        }
    }
}
