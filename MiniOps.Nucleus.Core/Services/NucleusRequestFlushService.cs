using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Hubs;
using Nucleus.Core.Stores;

namespace Nucleus.Core.Services;

public class NucleusRequestFlushService(
    IRequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestFlushService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(1);
        var table = $"{dbContext.Options.SchemaName}.RequestMetrics";

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var conn = dbContext.CreateConnection();
                await dbContext.OpenAsync(conn, stoppingToken);

                var sql = $@"
                    DELETE FROM {table}
                    WHERE Timestamp < @ExpiryTime;";

                var expiryTime = DateTime.UtcNow - TimeSpan.FromSeconds(dbContext.Options.LogTTLSeconds);

                var deletedCount = await conn.ExecuteAsync(sql, new { ExpiryTime = expiryTime });

                if (deletedCount > 0)
                {
                    logger.LogInformation(
                        "Deleted {Count} expired request logs older than {ExpiryTime:O}",
                        deletedCount,
                        expiryTime
                    );
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while deleting expired request metrics.");
                // Optional: backoff to avoid tight error loop
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
    
    
}
