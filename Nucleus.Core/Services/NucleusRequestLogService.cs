using Dapper;
using Microsoft.Extensions.Hosting;
using Nucleus.Core.Stores;

namespace Nucleus.Core.Services;

public class NucleusRequestLogService(
    IRequestStore store,
    NucleusDbContext dbContext) : BackgroundService
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

                // ✅ bulk insert in one query
                var sql = "INSERT INTO Nucleus.RequestMetrics (Id, Timestamp, DurationMs, StatusCode, Path) VALUES (@Id, @Timestamp, @DurationMs, @StatusCode, @Path);";

                await conn.ExecuteAsync(sql, logs);
            }

            await Task.Delay(interval, stoppingToken);
        }

    }
}