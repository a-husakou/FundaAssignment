using FundaAssignment.Application.TrendingMakelaarCalculation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using FundaAssignment.TrendingMakelaarApi;
using FundaAssignment.Infrastructure;

namespace FundaAssignment.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json"), optional: true, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // Set env vars used by configuration (both the wanted key and the actual config key)
            Environment.SetEnvironmentVariable("API-KEY", "TEST");
            Environment.SetEnvironmentVariable("FundaApi__ApiKey", "TEST");

            // Keep hosted services enabled so warmup/background processing can run during tests.
            // Intervals are shortened via appsettings.Test.json to keep tests fast.

            // Provide a Mock HTTP handler for external HTTP calls (IFundaApiClient)
            var mockHttp = new MockHttpMessageHandler();
            services.AddSingleton(mockHttp);

            // Re-register the typed client to use our mock handler as primary
            services.AddHttpClient<IFundaApiClient, FundaApiClient>()
                .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<MockHttpMessageHandler>())
                .AddHttpMessageHandler<RateLimitRetryHandler>();
        });
    }
}
