using System.Net.Http.Json;
using System.Text.Json;
using FundaAssignment.Application.TrendingMakelaarCalculation;

namespace FundaAssignment.Infrastructure;

public class FundaApiClient : IFundaApiClient
{
    private const int PageSize = 25;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;

    public FundaApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<FundaListingsResult> GetListingsBySearchTermAsync(string searchTerm, int pageNumber)
    {
        var requestUri = "?type=koop&zo={searchTerm}&page={pageNumber}&pagesize={PageSize}";
        // TODO handle errors
        var result = await httpClient.GetFromJsonAsync<FundaListingsResult>(requestUri, JsonOptions);
        return result!;
    }
}

