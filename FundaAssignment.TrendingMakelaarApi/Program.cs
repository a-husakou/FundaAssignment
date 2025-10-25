using FundaAssignment.Infrastructure;
using Funda.Common.Warmup.ShortCircuit;

namespace FundaAssignment.TrendingMakelaarApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register Infrastructure (includes background processor and dependencies)
            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Short-circuit all requests until warm-up completes (allows /health and /swagger)
            object value = app.UseWarmupShortCircuit();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
