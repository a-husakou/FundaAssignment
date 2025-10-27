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
using FundaAssignment.Infrastructure;

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

                void ConfigurePrimaryHandler(string name) 
                {
                    services.PostConfigure<HttpClientFactoryOptions>(name, opt => 
                    {
                        opt.HttpMessageHandlerBuilderActions.Add(builder => 
                        {
                            var mockHanlder = builder.Services.GetRequiredService<MockHttpMessageHandler>();
                            builder.PrimaryHandler = mockHanlder;
                        });
                    })
                ;}
                ConfigurePrimaryHandler(typeof(IFundaApiClient).Name);
                ConfigurePrimaryHandler(typeof(IFundaApiClient).FullName!);
                ConfigurePrimaryHandler(typeof(FundaApiClient).Name);
                ConfigurePrimaryHandler(typeof(FundaApiClient).FullName!);
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
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json"), optional: false, reloadOnChange: false);
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



