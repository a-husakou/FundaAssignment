using System.Text.Json.Serialization;

namespace FundaAssignment.Application.TrendingMakelaarCalculation;

public class FundaListingsDto
{
    // TODO Use fluent notation
    [JsonPropertyName("Objects")]
    public List<ListingData> Listings { get; init; }

    public PagingInfo Paging { get; init; }
}

public sealed class ListingData
{
    public int MakelaarId { get; init; }

    public string MakelaarNaam { get; init; }
}

public sealed class PagingInfo
{
    public int AantalPaginas { get; init; }

    public int HuidigePagina { get; init; }
}

