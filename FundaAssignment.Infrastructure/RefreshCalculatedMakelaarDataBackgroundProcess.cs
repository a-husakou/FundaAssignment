using Funda.Common.BackgroundProcessing;
using FundaAssignment.Application.TrendingMakelaarCalculation;

namespace FundaAssignment.Infrastructure
{
    public class RefreshCalculatedMakelaarDataBackgroundProcess : IBackgroundProcessor
    {
        private readonly TrendingMakelaarCalculationService trendingMakelaarCalculationService;

        public RefreshCalculatedMakelaarDataBackgroundProcess(TrendingMakelaarCalculationService trendingMakelaarCalculationService)
        {
            this.trendingMakelaarCalculationService = trendingMakelaarCalculationService;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            return trendingMakelaarCalculationService.RefreshTrendingMakelaarDataAsync(cancellationToken);
        }
    }
}
