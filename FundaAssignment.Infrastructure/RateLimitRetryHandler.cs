using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace FundaAssignment.Infrastructure;

// This handler could go to Funda.Common.Http (or similar) if needed elsewhere
public class RateLimitRetryHandler : DelegatingHandler
{
    private readonly TimeSpan rateLimitDelay;
    private readonly int maxRetries;

    public RateLimitRetryHandler(IOptions<FundaApiClientRetryOptions> options)
    {
        var cfg = options.Value;
        rateLimitDelay = cfg.RateLimitDelay;
        maxRetries = cfg.MaxRetries;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != (HttpStatusCode)429)
            {
                return response;
            }

            if (attempt == maxRetries)
            {
                return response;
            }

            var delay = rateLimitDelay;
            var retryAfter = GetDelayFromRetryAfter(response.Headers.RetryAfter);
            if (retryAfter.HasValue && retryAfter.Value > delay)
            {
                delay = retryAfter.Value;
            }

            response.Dispose();
            await Task.Delay(delay, cancellationToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static TimeSpan? GetDelayFromRetryAfter(RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter == null)
        {
            return null;
        }

        if (retryAfter.Delta.HasValue)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter.Date.HasValue)
        {
            var delta = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
        }

        return null;
    }
}
