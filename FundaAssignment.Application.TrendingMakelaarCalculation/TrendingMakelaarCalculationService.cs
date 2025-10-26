using FundaAssignment.Application.Common;
using Microsoft.Extensions.Options;

namespace FundaAssignment.Application.TrendingMakelaarCalculation;

public class TrendingMakelaarCalculationService
{
    private readonly IFundaApiClient fundaApiClient;
    private readonly ICalculatedResultStore calculatedResultStore;
    private readonly FilterConfig filterConfig;
    private readonly CalculationConfig calculationConfig;

    public TrendingMakelaarCalculationService(
        IFundaApiClient fundaApiClient,
        ICalculatedResultStore calculatedResultStore,
        IOptions<FilterConfig> filterConfig,
        IOptions<CalculationConfig> calculationConfig)
    {
        this.filterConfig = filterConfig.Value;
        this.calculationConfig = calculationConfig.Value;
        this.fundaApiClient = fundaApiClient;
        this.calculatedResultStore = calculatedResultStore;
    }

    // TODO scheduling via background service during warm up and then every configured interval according to appsettingss
    // TODO make sure warm up runs before any background services start
    public async Task RefreshTrendingMakelaarDataAsync(CancellationToken cancellationToken)
    {
        foreach (var searchTerm in filterConfig.FilterSearchTerms)
        {
            var data = await calculatedResultStore.GetCalculatedDataAsync(searchTerm);
            if (DateTime.UtcNow.Subtract(data.CalculatedAtUtc) > calculationConfig.RefreshInterval)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await CalculateAndStoreTrendingMakelaarAsync(searchTerm);
            }
        }
    }

    // TODO add to readme an explanation that this method will be awaited for couple of minutes (3 to be exact) and that it executes in the background
    // Intermediary results are kept in memory to keep things simple, scaling to multiple deployments is not needed as we serve only single fetch endpoint from cache/quick db, so a single server can already support numerous sceanarios
    public async Task CalculateAndStoreTrendingMakelaarAsync(string searchTerm)
    {
        var pageNumber = 1;
        var lastPageFetched = false;
        var makelaarCountMap = new Dictionary<MakelaarInfo, int>();

        while (!lastPageFetched)
        {
            // TODO Make 429 to be part of HTTP client handling with retry and backoff (external lib?) so it is not propogated here
            // TODO Other problems should be captured and when hosts detects that Refresh fails, it needs to alert and schedule retry sooner than regular interval
            var fundaListingsDto = await fundaApiClient.GetListingsBySearchTermAsync(searchTerm, pageNumber);

            foreach (var listing in fundaListingsDto.Objects)
            {
                var makelaarInfo = new MakelaarInfo(listing.MakelaarId, listing.MakelaarNaam);
                if (makelaarCountMap.ContainsKey(makelaarInfo))
                {
                    makelaarCountMap[makelaarInfo]++;
                }
                else
                {
                    makelaarCountMap[makelaarInfo] = 1;
                }
            }

            lastPageFetched = pageNumber++ == fundaListingsDto.Paging.AantalPaginas;
        }

        // TODO add metrics for listing count per makelaar
        var sortedList = new SortedList<int, MakelaarInfo>();
        foreach (var pair in makelaarCountMap)
        {
            sortedList.Add(pair.Value, pair.Key);
        }

        // TODO add the timestamp of calculation to be able to monitor staleness of data and skip it during warm up if needed
        await calculatedResultStore.StoreMakelaarItemsAsync(searchTerm, sortedList);
    }
}
