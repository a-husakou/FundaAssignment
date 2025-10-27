using System.Net;
using FundaAssignment.Tests.Infrastructure;
using FluentAssertions;
using Xunit;
using RichardSzalay.MockHttp;
using FundaAssignment.Tests.Utils;
using Xunit.Abstractions;
using System.Net.Http.Json;
using FundaAssignment.Application.Common;

namespace FundaAssignment.Tests;

public class ApiTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory factory;
    private readonly ITestOutputHelper output;

    public ApiTest(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        this.factory = factory;
        this.output = output;
    }

    [Fact]
    public async Task InitialCalculation_ShouldCauseRequestShortCircuit()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "*").Respond(HttpStatusCode.TooManyRequests);

        var client = factory.WithMockHttp(mockHttp).CreateClient();
        var resp = await client.GetAsync("/TrendingMakelaar?filter=%2Famsterdam");

        resp.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task ResultCalculation_ShouldRespectPagingAndRateLimiting()
    {
        var mockHttp = CreatePreconfiguredHandler();
        mockHttp.Expect(HttpMethod.Get, "*")
            .WithQueryString("zo", "/amsterdam")
            .RespondWith(builder => builder
                .WithTotalPages(2)
                .AddListings("Makelaar A", "Makelaar B"));
        mockHttp.Expect(HttpMethod.Get, "*")
            .WithQueryString("zo", "/amsterdam")
            .Respond(HttpStatusCode.TooManyRequests);
        mockHttp.Expect(HttpMethod.Get, "*")
            .WithQueryString("zo", "/amsterdam")
            .RespondWith(builder => builder
                .WithTotalPages(2)
                .AddListings("Makelaar A", "Makelaar B"));

        var client = factory.WithMockHttp(mockHttp).CreateClient();
        await AssertSuccessfulCalculation(client, calculatedData =>
        {
            calculatedData.DescendingSortedItems.Should().HaveCount(2);

            calculatedData.DescendingSortedItems.Should().Contain(x => x.Makelaar.Name == "Makelaar A" && x.TotalListings == 2);
            calculatedData.DescendingSortedItems.Should().Contain(x => x.Makelaar.Name == "Makelaar B" && x.TotalListings == 2);
        });
    }

    [Fact]
    public async Task ResultCalculation_ShouldRefreshPeriodically()
    {
        var mockHttp = CreatePreconfiguredHandler();
        mockHttp.Expect(HttpMethod.Get, "*")
            .WithQueryString("zo", "/amsterdam")
            .RespondWith(builder => builder
                .WithTotalPages(1)
                .AddListings("Makelaar A", "Makelaar B"));
        mockHttp.Expect(HttpMethod.Get, "*")
            .WithQueryString("zo", "/amsterdam")
            .RespondWith(builder => builder
                .WithTotalPages(1)
                .AddListing(1, "Makelaar A")
                .AddListing(1, "Makelaar A"));

        var client = factory.WithMockHttp(mockHttp).CreateClient();
        await AssertSuccessfulCalculation(client, calculatedData =>
        {
            calculatedData.DescendingSortedItems.Should().HaveCount(2);
            calculatedData.DescendingSortedItems.Should().Contain(x => x.Makelaar.Name == "Makelaar A" && x.TotalListings == 1);
            calculatedData.DescendingSortedItems.Should().Contain(x => x.Makelaar.Name == "Makelaar B" && x.TotalListings == 1);
        });

        await AssertSuccessfulCalculation(client, calculatedData =>
        {
            calculatedData.DescendingSortedItems.Should().HaveCount(1);
            calculatedData.DescendingSortedItems.Should().Contain(x => x.Makelaar.Name == "Makelaar A" && x.TotalListings == 2);
        });
    }

    private Task AssertSuccessfulCalculation(HttpClient client, Action<CalculatedMakelaarData> assertAction)
    {
        return PeriodicAssert.Create().ExecuteAsync(async () =>
        {
            var statusResp = await client.GetAsync("/TrendingMakelaar/?filter=%2Famsterdam");
            statusResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var data = await statusResp.Content.ReadFromJsonAsync<CalculatedMakelaarData>();
            data.Should().NotBeNull();
            assertAction(data);
        });
    }

    private MockHttpMessageHandler CreatePreconfiguredHandler()
    {
        // for /amsterdam/tuin filter
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback
            .RespondWith(builder => builder
                .WithTotalPages(1)
                .AddListing(1, "Makelaar C"));

        return mockHttp;
    }
}

