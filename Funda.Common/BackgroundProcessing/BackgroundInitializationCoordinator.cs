
using System.Collections.Concurrent;

namespace Funda.Common.BackgroundProcessing;

public class BackgroundInitializationCoordinator : IInitializationState
{
    private readonly ConcurrentDictionary<Type, bool> initializedProcessors = new();

    public IReadOnlyCollection<Type> InitializedProcessors => initializedProcessors.Keys.ToList();

    public void ReportInitialized(Type processorType)
    {
        initializedProcessors[processorType] = true;
    }
}
