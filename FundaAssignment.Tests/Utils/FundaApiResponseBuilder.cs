using System.Net;
using System.Text;
using System.Text.Json;
using FundaAssignment.Application.TrendingMakelaarCalculation;
using RichardSzalay.MockHttp;

namespace FundaAssignment.Tests.Utils;

public sealed class FundaApiResponseBuilder
{
    private readonly List<ListingData> objects = new();
    private int totalPages = 1;
    private int currentPage = 1;

    public FundaApiResponseBuilder WithCurrentPage(int page)
    {
        currentPage = page;
        return this;
    }

    public FundaApiResponseBuilder WithTotalPages(int pages)
    {
        totalPages = pages;
        return this;
    }

    public FundaApiResponseBuilder AddListing(int makelaarId, string makelaarNaam)
    {
        objects.Add(new ListingData { MakelaarId = makelaarId, MakelaarNaam = makelaarNaam });
        return this;
    }

    public FundaApiResponseBuilder AddListings(params string[] makelaarNamen)
    {
        var startId = objects.Count + 1;
        for (int i = 0; i < makelaarNamen.Length; i++)
        {
            AddListing(startId + i, makelaarNamen[i]);
        }
        return this;
    }

    public FundaListingsResult Build()
        => new FundaListingsResult
        {
            Objects = objects,
            Paging = new PagingInfo { AantalPaginas = totalPages, HuidigePagina = currentPage }
        };

    public string BuildJson()
        => JsonSerializer.Serialize(Build());

    public HttpResponseMessage ToHttpResponse(HttpStatusCode status = HttpStatusCode.OK)
        => new HttpResponseMessage(status)
        {
            Content = new StringContent(BuildJson(), Encoding.UTF8, "application/json")
        };
}

public static class MockHttpFundaExtensions
{
    public static MockedRequest RespondWith(this MockedRequest request, Action<FundaApiResponseBuilder> setupBuilder)
    {
        var builder = new FundaApiResponseBuilder();
        setupBuilder(builder);
        return request.Respond("application/json", builder.BuildJson());
    }
}

