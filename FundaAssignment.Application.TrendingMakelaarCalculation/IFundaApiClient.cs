namespace FundaAssignment.Application.TrendingMakelaarCalculation;

public interface IFundaApiClient
{
    Task<FundaListingsResult> GetListingsBySearchTermAsync(string searchTerm, int pageNumber);
}
