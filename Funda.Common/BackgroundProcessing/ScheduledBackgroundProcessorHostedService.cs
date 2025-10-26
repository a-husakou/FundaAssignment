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
    private readonly BackgroundRetryOptions retryOptions;
    private readonly ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger;

    public ScheduledBackgroundProcessorHostedService(
        IServiceProvider serviceProvider,
        TimeSpan interval,
        bool performInitializationRun,
        BackgroundInitializationCoordinator initializationCoordinator,
        ILogger<ScheduledBackgroundProcessorHostedService<TProcessor>> logger,
        BackgroundRetryOptions? retryOptions = null)
    {
        this.serviceProvider = serviceProvider;
        this.interval = interval;
        this.performInitializationRun = performInitializationRun;
        this.initializationCoordinator = initializationCoordinator;
        this.logger = logger;
        this.retryOptions = retryOptions ?? new BackgroundRetryOptions();
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
        int attempt = 1;
        var delay = retryOptions.InitialDelay;
        while (true)
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
                logger.LogError(ex, "Run of background processor {Processor} failed on attempt {Attempt}.", typeof(TProcessor).Name, attempt);

                if (attempt >= Math.Max(1, retryOptions.MaxAttempts))
                {
                    logger.LogWarning("Max attempts reached for background processor {Processor}. Giving up until next schedule.", typeof(TProcessor).Name);
                    return false;
                }

                // compute next delay with backoff and cap
                var effectiveDelay = delay;
                if (retryOptions.BackoffFactor > 0)
                {
                    var next = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * retryOptions.BackoffFactor);
                    delay = next > retryOptions.MaxDelay ? retryOptions.MaxDelay : next;
                }

                try
                {
                    logger.LogInformation("Retrying background processor {Processor} in {Delay} (attempt {NextAttempt}/{MaxAttempts}).", typeof(TProcessor).Name, effectiveDelay, attempt + 1, retryOptions.MaxAttempts);
                    await Task.Delay(effectiveDelay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return false;
                }

                attempt++;
            }
        }
    }
}
