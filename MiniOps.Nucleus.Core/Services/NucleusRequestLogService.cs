using System.Diagnostics;
using System.Collections.Concurrent;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Hubs;
using Nucleus.Core.Models;
using Nucleus.Core.Stores;
using Z.Dapper.Plus;

namespace Nucleus.Core.Services;


public sealed class NucleusRequestLogService(
    MemoryRequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestLogService> logger, 
    IHubContext<NucleusHub> hub) : BackgroundService
{
    private NucleusDbContext? _connection;

    private class Aggregate
    {
        public int TotalRequests;
        public int SuccessRequests;
        public int FailedRequests;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchInterval = TimeSpan.FromSeconds(dbContext.Options.BatchFlushIntervalSeconds);
        var aggregateInterval = TimeSpan.FromSeconds(dbContext.Options.BatchFlushIntervalSeconds); // for SignalR
        var conn = dbContext.CreateConnection();
        await dbContext.OpenAsync(conn, stoppingToken);

        var batch = new List<NucleusLog>();
        var nextBatchFlush = DateTime.UtcNow.Add(batchInterval);
        var nextAggregateSend = DateTime.UtcNow.Add(aggregateInterval);

        int totalRequests = 0;
        int totalSuccess = 0;
        int totalFailed = 0;

        await foreach (var log in store.ReadAllAsync(stoppingToken))
        {
            batch.Add(log);

            // Update per-second aggregates
            totalRequests++;
            if (log.StatusCode == 200) totalSuccess++;
            else totalFailed++;

            var now = DateTime.UtcNow;

            // Send per-second aggregate to SignalR
            if (now >= nextAggregateSend)
            {
                await hub.Clients.All.SendAsync("ReceiveMetrics", new
                {
                    totalRequests,
                    totalSuccessRequests = totalSuccess,
                    totalFailedRequests = totalFailed
                }, cancellationToken: stoppingToken);

                // Reset counters for the next cycle
                totalRequests = 0;
                totalSuccess = 0;
                totalFailed = 0;

                nextAggregateSend = now.Add(aggregateInterval);
            }

            // Flush batch to DB if interval elapsed
            if (now >= nextBatchFlush)
            {
                if (batch.Count > 0)
                {
                    var stopwatch = Stopwatch.StartNew();
                    await conn.BulkInsertAsync(batch);
                    stopwatch.Stop();
                    logger.LogInformation("Inserted {Count} logs in {ElapsedMs} ms", batch.Count, stopwatch.ElapsedMilliseconds);
                    batch.Clear();
                }

                nextBatchFlush = now.Add(batchInterval);
            }
        }

        // Final flush on shutdown
        if (batch.Count > 0)
        {
            await conn.BulkInsertAsync(batch);
        }
    }
    //
    // public override async Task StopAsync(CancellationToken cancellationToken)
    // {
    //     var remainingLogs = store.Flush();
    //     if (remainingLogs.Count > 0)
    //     {
    //         using var conn = dbContext.CreateConnection();
    //         await dbContext.OpenAsync(conn, cancellationToken);
    //
    //         try
    //         {
    //             await conn.BulkInsertAsync(remainingLogs);
    //             logger.LogInformation("Flushed {Count} logs on shutdown", remainingLogs.Count);
    //         }
    //         catch (Exception ex)
    //         {
    //             logger.LogError(ex, "Failed to flush logs on shutdown.");
    //         }
    //     }
    //
    //     await base.StopAsync(cancellationToken);
    // }
}
