using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Stores;
using Z.Dapper.Plus;

namespace Nucleus.Core.Services;

public class NucleusRequestLogService(
    IRequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestLogService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(dbContext.Options.BatchFlushIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var logs = store.Flush();

            if (logs.Count > 0)
            {
                using var conn = dbContext.CreateConnection();
                await dbContext.OpenAsync(conn, stoppingToken);

                var stopwatch = Stopwatch.StartNew();

                // ✅ bulk insert in one query
                var sql = "INSERT INTO Nucleus.RequestMetrics (Id, Timestamp, DurationMs, StatusCode, Path) " +
                          "VALUES (@Id, @Timestamp, @DurationMs, @StatusCode, @Path);";

                await conn.BulkInsertAsync(logs);

                stopwatch.Stop();

                logger.LogInformation(
                    "Inserted {Count} request logs in {ElapsedMs} ms",
                    logs.Count,
                    stopwatch.ElapsedMilliseconds);
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var remainingLogs = store.Flush();
        if (remainingLogs.Count > 0)
        {
            using var conn = dbContext.CreateConnection();
            await dbContext.OpenAsync(conn, cancellationToken);

            try
            {
                await conn.BulkInsertAsync(remainingLogs);
                logger.LogInformation("Flushed {Count} logs on shutdown", remainingLogs.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to flush logs on shutdown.");
                // optional: write to disk/queue for retry later
            }
        }

        await base.StopAsync(cancellationToken);
    }
}