using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Hubs;
using Nucleus.Core.Stores;
using Z.Dapper.Plus;

namespace Nucleus.Core.Services;

public sealed class NucleusRequestLogService(
    RequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestLogService> logger, 
    IHubContext<NucleusHub> hub) : BackgroundService
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

                await conn.BulkInsertAsync(logs);

                stopwatch.Stop();
                await hub.Clients.All.SendAsync("ReceiveMetrics", new {
                    totalRequests = logs.Count,
                    totalSuccessRequests = logs.Count(x => x.StatusCode == 200),
                    totalFailedRequests = logs.Count(x=>x.StatusCode != 200)
                }, cancellationToken: stoppingToken);
                
                logger.LogInformation(
                    "Inserted {Count} request logs in {ElapsedMs} ms",
                    logs.Count,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                await hub.Clients.All.SendAsync("ReceiveMetrics", new {
                    totalRequests = 0,
                    totalSuccessRequests = 0,
                    totalFailedRequests = 0
                }, cancellationToken: stoppingToken);
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
            }
        }

        await base.StopAsync(cancellationToken);
    }
}