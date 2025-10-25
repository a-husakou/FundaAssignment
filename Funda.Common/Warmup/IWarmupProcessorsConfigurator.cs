using Funda.Common.BackgroundProcessing;

namespace Funda.Common.Warmup;

public interface IWarmupConfigurator
{
    void AddRequiredProcessor<TProcessor>() where TProcessor : IBackgroundProcessor;
}
