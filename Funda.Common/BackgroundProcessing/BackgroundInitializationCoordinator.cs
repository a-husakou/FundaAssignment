using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Funda.Common.BackgroundProcessing;

public class BackgroundInitializationCoordinator : IInitializationState
{
    private readonly ConcurrentDictionary<Type, bool> initializedProcessors = new();
    private readonly ILogger<BackgroundInitializationCoordinator> logger;

    public BackgroundInitializationCoordinator(ILogger<BackgroundInitializationCoordinator> logger)
    {
        this.logger = logger;
    }

    public IReadOnlyCollection<Type> InitializedProcessors => initializedProcessors.Keys.ToList();

    public void ReportInitialized(Type processorType)
    {
        if (initializedProcessors.TryAdd(processorType, true))
        {
            logger.LogInformation("Background processor initialized: {Processor}", processorType.FullName);
        }
        else
        {
            logger.LogDebug("Background processor initialization reported again: {Processor}", processorType.FullName);
        }
    }
}
