using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nucleus.Core.Stores;

namespace Nucleus.Core.Services;

public class NucleusRequestFlushService(
    IRequestStore store,
    NucleusDbContext dbContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var conn = dbContext.CreateConnection();
            await dbContext.OpenAsync(conn, stoppingToken);

            // ✅ bulk insert in one query
            var sql = @"
                DELETE FROM Nucleus.RequestMetrics
                WHERE Timestamp < @ExpiryTime;
            ";

            var expiryTime = DateTime.UtcNow - TimeSpan.FromSeconds(dbContext.Options.LogTTLSeconds);

            await conn.ExecuteAsync(sql, new { ExpiryTime = expiryTime });

            await Task.Delay(interval, stoppingToken);
        }
    }
}
