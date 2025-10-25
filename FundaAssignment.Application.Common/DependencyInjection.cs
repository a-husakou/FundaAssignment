using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FundaAssignment.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCommon(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<FilterConfig>()
            .Bind(configuration.GetSection("Filters"))
            .Validate(o => o.FilterSearchTerms != null, "Filters.FilterSearchTerms is required")
            .ValidateOnStart();
        return services;
    }
}
