using System.Diagnostics;
using Nucleus.Core;
using Nucleus.Core.Config;
using Nucleus.Dashboard.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNucleus(op =>
{
    op.LogTTLSeconds = 60;
    op.BatchFlushIntervalSeconds = 1;
    op.DatabaseType = NucleusDatabaseTypes.SQLServer;
    op.SchemaName = "Nucleus";
    op.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                          throw new InvalidOperationException();
    op.SeedDatabase = true;
});

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
        // 25% chance to fail
        if (Random.Shared.NextDouble() < 0.25)
        {
            // This will return a 500 Internal Server Error
            throw new Exception("Simulated server error");
        }

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        return forecast;
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