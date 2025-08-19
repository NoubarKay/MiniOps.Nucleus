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
    RequestStore store,
    NucleusDbContext dbContext,
    ILogger<NucleusRequestLogService> logger, 
    IHubContext<NucleusHub> hub) : BackgroundService
{
    private readonly ConcurrentDictionary<DateTime, Aggregate> _inMemoryAggregates = new();

    private class Aggregate
    {
        public int TotalRequests;
        public int SuccessRequests;
        public int FailedRequests;
    }

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

                foreach (var log in logs)
                {
                    var bucket = log.Timestamp.AddTicks(-(log.Timestamp.Ticks % TimeSpan.TicksPerSecond));
                    var agg = _inMemoryAggregates.GetOrAdd(bucket, _ => new Aggregate());

                    agg.TotalRequests++;
                    if (log.StatusCode == 200)
                        agg.SuccessRequests++;
                    else
                        agg.FailedRequests++;
                }

                var stopwatch = Stopwatch.StartNew();

                var insertLogsTask = conn.BulkInsertAsync(logs);

                var aggregatesToInsert = _inMemoryAggregates.Select(kvp => new
                {
                    BucketTime = kvp.Key,
                    TotalRequests = kvp.Value.TotalRequests,
                    SuccessRequests = kvp.Value.SuccessRequests,
                    FailedRequests = kvp.Value.FailedRequests
                }).ToList();

                var sql = @"
                MERGE [Nucleus].[RequestAggregates] AS target
                USING (VALUES (@BucketTime, @TotalRequests, @SuccessRequests, @FailedRequests)) 
                    AS source (BucketTime, TotalRequests, SuccessRequests, FailedRequests)
                ON target.BucketTime = source.BucketTime
                WHEN MATCHED THEN 
                    UPDATE SET 
                        TotalRequests = target.TotalRequests + source.TotalRequests,
                        SuccessRequests = target.SuccessRequests + source.SuccessRequests,
                        FailedRequests = target.FailedRequests + source.FailedRequests
                WHEN NOT MATCHED THEN
                    INSERT (BucketTime, TotalRequests, SuccessRequests, FailedRequests)
                    VALUES (source.BucketTime, source.TotalRequests, source.SuccessRequests, source.FailedRequests);";

                var insertAggregatesTask = conn.ExecuteAsync(sql, aggregatesToInsert);

                await Task.WhenAll(insertLogsTask, insertAggregatesTask);

                stopwatch.Stop();

                var totalRequests = logs.Count;
                var totalSuccess = logs.Count(x => x.StatusCode == 200);
                var totalFailed = logs.Count(x => x.StatusCode != 200);

                await hub.Clients.All.SendAsync("ReceiveMetrics", new {
                    totalRequests,
                    totalSuccessRequests = totalSuccess,
                    totalFailedRequests = totalFailed
                }, cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Inserted {Count} logs in {ElapsedMs} ms",
                    totalRequests,
                    stopwatch.ElapsedMilliseconds);

                _inMemoryAggregates.Clear();
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
