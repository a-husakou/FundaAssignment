using Funda.Common.BackgroundProcessing;
using FundaAssignment.Application.Common;
using FundaAssignment.Application.TrendingMakelaarCalculation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FundaAssignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register application-layer services and configs
        services.AddApplicationCommon(configuration);
        services.AddTrendingMakelaarCalculation(configuration);

        services.AddBackgroundProcessor<RefreshCalculatedMakelaarDataBackgroundProcess>(
            configuration.GetSection("BackgroundProcessing:RefreshCalculatedMakelaarData"));

        return services;
    }
}
