using Funda.Common.BackgroundProcessing;
using Funda.Common.Warmup;
using FundaAssignment.Application.Common;
using FundaAssignment.Application.TrendingMakelaarCalculation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FundaAssignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationCommon(configuration);
        services.AddTrendingMakelaarCalculation(configuration);

        if (!Uri.TryCreate(GetFundaUri(configuration), UriKind.Absolute, out var fundaBaseUri))
        {
            throw new InvalidOperationException("FundaApi:BaseUrl must be an absolute URL.");
        }
        services
            .AddOptions<FundaApiClientRetryOptions>()
            .Bind(configuration.GetSection("FundaApi:Retry"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddTransient<RateLimitRetryHandler>();
        services.AddHttpClient<IFundaApiClient, FundaApiClient>((sp, client) =>
        {
            client.BaseAddress = fundaBaseUri;
        })
        .AddHttpMessageHandler<RateLimitRetryHandler>();

        services.AddBackgroundProcessor<RefreshCalculatedMakelaarDataBackgroundProcess>(
            configuration.GetSection("BackgroundProcessing:RefreshCalculatedMakelaarData"));
        services.AddWarmup(configurator => configurator.AddRequiredProcessor<RefreshCalculatedMakelaarDataBackgroundProcess>());

        return services;
    }

    private static string GetFundaUri(IConfiguration configuration)
    {
        var baseUrl = configuration.GetValue<string>("FundaApi:BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("FundaApi:BaseUrl configuration is missing.");
        }
        var apiKey = configuration.GetValue<string>("FundaApi:ApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("FundaApi:ApiKey configuration is missing.");
        }
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }
        if (!apiKey.EndsWith("/", StringComparison.Ordinal))
        {
            apiKey += "/";
        }
        return $"{baseUrl}{apiKey}";
    }
}
