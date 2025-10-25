namespace Funda.Common.BackgroundProcessing
{
    public interface IBackgroundProcessor
    {
        Task Execute(CancellationToken cancellationToken);
    }
}

