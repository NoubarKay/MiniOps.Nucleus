using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nucleus.Core.Config;
using Nucleus.Core.Interfaces;
using Nucleus.Core.Middleware;
using Nucleus.Core.Services;

namespace Nucleus.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddNucleus(this IServiceCollection services, Action<NucleusOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NucleusOptions();
        configure(options);
        
        // Register options as singleton
        services.AddSingleton(options);
        
        // Register the central MiniOpsContext (the "Nucleus")
        services.AddSingleton<NucleusDbContext>(sp => new NucleusDbContext(new NucleusOptions
        {
            DatabaseType = options.DatabaseType,
            ConnectionString = options.ConnectionString,
            LogTTLSeconds = options.LogTTLSeconds,
            SeedDatabase = options.SeedDatabase
            // EnableSimulation = options.EnableSimulation,
            // DefaultSimulatedLatencyMs = options.DefaultSimulatedLatencyMs,
            // DefaultSimulatedFailureRate = options.DefaultSimulatedFailureRate,
            // EnableVerboseLogging = options.EnableVerboseLogging,
            // RequestLogTableName = options.RequestLogTableName,
            // DbCommandTimeoutSeconds = options.DbCommandTimeoutSeconds
        }));
        
        services.AddTransient<INucleusLogStore, NucleusLogStore>();
        
        return services;
    }
    
    /// <summary>
    /// Adds the Nucleus request tracking middleware to the ASP.NET Core pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated application builder.</returns>
    public static async Task<IApplicationBuilder> UseNucleus(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NucleusDbContext>();

            if (dbContext.Options.SeedDatabase)
            {
                await dbContext.EnsureNucleusDatabase(dbContext.Options.ConnectionString);
            }
        }

        // Adds your NucleusTrackerMiddleware to the pipeline
        return app.UseMiddleware<NucleusTrackerMiddleware>();
    }
}