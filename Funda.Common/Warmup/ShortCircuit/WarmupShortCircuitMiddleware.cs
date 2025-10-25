using System.Net;
using Funda.Common.Warmup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Funda.Common.Warmup.ShortCircuit;

public class WarmupShortCircuitMiddleware
{
    private readonly RequestDelegate next;
    private readonly WarmupCoordinator warmup;
    private static readonly string[] DefaultAllowedPrefixes = new[] { "/health", "/swagger" };

    public WarmupShortCircuitMiddleware(RequestDelegate next, WarmupCoordinator warmup)
    {
        this.next = next;
        this.warmup = warmup;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (warmup.IsInitialized || IsAllowedPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        context.Response.Headers["Retry-After"] = "30";
        await context.Response.WriteAsJsonAsync(new
        {
            status = "initializing",
        });
    }

    private static bool IsAllowedPath(PathString path)
        => DefaultAllowedPrefixes.Any(p => path.StartsWithSegments(p));
}

public static class WarmupShortCircuitMiddlewareExtensions
{
    public static IApplicationBuilder UseWarmupShortCircuit(this IApplicationBuilder app)
        => app.UseMiddleware<WarmupShortCircuitMiddleware>();
}

