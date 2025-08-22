using System.Diagnostics;
using System.Collections.Concurrent;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Hubs;
using Nucleus.Core.Stores;
using Z.Dapper.Plus;

namespace Nucleus.Core.Services;

public sealed class NucleusRequestLogService(
    IRequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestLogService> logger, 
    IHubContext<NucleusHub> hub) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(dbContext.Options.BatchFlushIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var logs = await store.ReadAllAsync(stoppingToken);

            if (logs.Count > 0)
            {
                using var conn = dbContext.CreateConnection();
                await dbContext.OpenAsync(conn, stoppingToken);

                var stopwatch = Stopwatch.StartNew();
                var insertLogsTask = conn.BulkInsertAsync(logs);
                stopwatch.Stop();

                var totalRequests = logs.Count;
                var totalSuccess = logs.Count(x => x.StatusCode == 200);
                var totalFailed = logs.Count(x => x.StatusCode != 200);

                await hub.Clients.All.SendAsync("ReceiveMetrics", new {
                    totalRequests,
                    totalSuccessRequests = totalSuccess,
                    totalFailedRequests = totalFailed
                }, cancellationToken: stoppingToken);
                

                // logger.LogInformation(
                //     "Inserted {Count} request logs in {ElapsedMs} ms",
                //     totalRequests,
                //     stopwatch.ElapsedMilliseconds);
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
    
}

