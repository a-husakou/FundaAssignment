using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FundaAssignment.Application.TrendingMakelaarCalculation;

public static class DependencyInjection
{
    public static IServiceCollection AddTrendingMakelaarCalculation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<CalculationConfig>()
            .Bind(configuration.GetSection("TrendingMakelaarCalculation"))
            .Validate(o => o.RefreshInterval > TimeSpan.Zero, "TrendingMakelaarCalculation.RefreshInterval must be greater than 00:00:00")
            .ValidateOnStart();
        services.AddTransient<TrendingMakelaarCalculationService>();
        return services;
    }
}
