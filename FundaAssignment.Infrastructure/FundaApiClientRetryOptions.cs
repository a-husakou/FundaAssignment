using System.ComponentModel.DataAnnotations;

namespace FundaAssignment.Infrastructure;

public class FundaApiClientRetryOptions
{
    [Range(typeof(TimeSpan), "00:00:00.100", "365.00:00:00")]
    public TimeSpan RateLimitDelay { get; set; } = TimeSpan.FromMinutes(1);

    [Range(1, int.MaxValue)]
    public int MaxRetries { get; set; } = 3;
}
