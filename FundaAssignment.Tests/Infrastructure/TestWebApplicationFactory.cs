using FundaAssignment.Application.TrendingMakelaarCalculation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using FundaAssignment.TrendingMakelaarApi;
using Microsoft.Extensions.Http;

namespace FundaAssignment.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public WebApplicationFactory<Program> WithMockHttp(MockHttpMessageHandler mock)
    {
        return this.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mock);
                services.PostConfigure<HttpClientFactoryOptions>(typeof(IFundaApiClient).FullName!, options =>
                {
                    options.HttpMessageHandlerBuilderActions.Add(b =>
                    {
                        var m = b.Services.GetRequiredService<MockHttpMessageHandler>();
                        b.PrimaryHandler = m;
                    });
                });
            });
        });
    }

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
            // Do not re-register IFundaApiClient here to keep production pipeline intact.
        });
    }
}
