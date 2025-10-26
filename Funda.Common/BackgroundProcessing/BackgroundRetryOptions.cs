using System.ComponentModel.DataAnnotations;

namespace Funda.Common.BackgroundProcessing;

public class BackgroundRetryOptions
{
    [Range(1, int.MaxValue)]
    public int MaxAttempts { get; set; } = 1; // 1 = no retries, just initial attempt

    [Range(typeof(TimeSpan), "00:00:01", "365.00:00:00")]
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(30);

    [Range(double.Epsilon, double.MaxValue)]
    public double BackoffFactor { get; set; } = 2.0; // exponential backoff multiplier

    [Range(typeof(TimeSpan), "00:00:01", "365.00:00:00")]
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
}
