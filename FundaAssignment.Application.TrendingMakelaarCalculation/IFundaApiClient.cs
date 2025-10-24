namespace FundaAssignment.Application.TrendingMakelaarCalculation;

// TODO implementation to encapsulate page size via config, add explanation to readme that the balanced page size is selected upfront 
public interface IFundaApiClient
{
    Task<FundaListingsDto> GetListingsBySearchTermAsync(string searchTerm, int pageNumber);
}

