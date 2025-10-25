using Funda.Common.BackgroundProcessing;

namespace Funda.Common.Warmup;

public class WarmupCoordinator
{
    private readonly IInitializationState initializationState;
    private readonly IEnumerable<Type> requiredProcessors;

    public WarmupCoordinator(IInitializationState initializationState, IEnumerable<Type> requiredProcessors)
    {
        this.initializationState = initializationState;
        this.requiredProcessors = requiredProcessors;
    }

    public bool IsInitialized =>
        requiredProcessors.Except(initializationState.InitializedProcessors).Any() == false;
}
