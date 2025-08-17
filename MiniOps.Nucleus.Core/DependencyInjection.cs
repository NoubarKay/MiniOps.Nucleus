using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Nucleus.Core.Config;
using Nucleus.Core.Hubs;
using Nucleus.Core.Middleware;
using Nucleus.Core.Models;
using Nucleus.Core.Services;
using Nucleus.Core.Stores;
using Z.Dapper.Plus;
using Microsoft.Extensions.DependencyInjection;

namespace Nucleus.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddNucleus(this IServiceCollection services, Action<NucleusOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        
        DapperPlusManager.Entity<NucleusLog>()
            .Table("Nucleus.RequestMetrics");

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
            BatchFlushIntervalSeconds = options.BatchFlushIntervalSeconds,
            SeedDatabase = options.SeedDatabase
        }));

        services.AddScoped<SeedService>();
        services.AddSingleton<RequestStore>();
        services.AddHostedService<NucleusRequestFlushService>();
        services.AddHostedService<NucleusRequestLogService>();
        
        return services;
    }
    
    /// <summary>
    /// Adds the Nucleus request tracking middleware to the ASP.NET Core pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated application builder.</returns>
    public static async Task UseNucleus(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NucleusDbContext>();
            var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();

            if (dbContext.Options.SeedDatabase)
            {
                await dbContext.EnsureNucleusDatabase(dbContext.Options.ConnectionString, seedService.GetSeed(dbContext.Options.DatabaseType));
            }
        }

        app.UseMiddleware<NucleusTrackerMiddleware>();
    }
}