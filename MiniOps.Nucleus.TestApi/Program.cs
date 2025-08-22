using System.Diagnostics;
using MiniOps.Nucleus.Redis.DependencyInjection;
using Nucleus.Core;
using Nucleus.Core.Config;
using Nucleus.Dashboard.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNucleus(nucleus => nucleus
    .UseDatabase(NucleusDatabaseTypes.SQLServer, 
        builder.Configuration.GetConnectionString("DefaultConnection")!)
    .WithSchema("Nucleus")
    .EnableSeedDatabase()
    .SetLogTtl(60)
    .SetBatchFlushInterval(1)
    .WithCustomTables("RequestMetrics",  "RequestAggregates")
).AddNucleusRedis(op =>
{
    // op.ConnectionString = "default:djjB7Pq8frMm3RMZWX6P2FZEkiI5bMg7cuZwb11ny8eiwsTUeqcMUOPLpCFlX603@69.62.116.35:5412/0";
    op.ConnectionString = "user=default,password=djjB7Pq8frMm3RMZWX6P2FZEkiI5bMg7cuZwb11ny8eiwsTUeqcMUOPLpCFlX603,69.62.116.35:5412";
});
builder.Logging.AddFilter("Nucleus", LogLevel.Information);
builder.Services.AddNucleusDashboardService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        // 25% chance to fail with 500
        if (Random.Shared.NextDouble() < 0.25)
        {
            return Results.BadRequest("Simulated server error");
        }

        // 25% chance to return 404
        if (Random.Shared.NextDouble() < 0.25)
        {
            return Results.NotFound(new { Message = "Simulated not found" });
        }

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        return Results.Ok(forecast);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

await app.UseNucleus();
app.UseNucleusDashboard("/dashboard");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}