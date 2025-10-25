using Funda.Common.BackgroundProcessing;

namespace Funda.Common.Warmup;

public class WarmupConfigurator : IWarmupConfigurator
{
    public List<Type> RequiredProcessors { get; } = new();

    public void AddRequiredProcessor<TProcessor>() where TProcessor : IBackgroundProcessor
    {
        RequiredProcessors.Add(typeof(TProcessor));
    }
}