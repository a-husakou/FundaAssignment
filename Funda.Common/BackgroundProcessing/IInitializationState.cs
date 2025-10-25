namespace Funda.Common.BackgroundProcessing;

public interface IInitializationState
{
    IReadOnlyCollection<Type> InitializedProcessors { get; }
}
