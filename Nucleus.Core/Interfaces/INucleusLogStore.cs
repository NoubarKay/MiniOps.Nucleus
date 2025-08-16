using Dapper;
using Nucleus.Core.Models;
using Nucleus.Core.Services;

namespace Nucleus.Core.Interfaces;

public interface INucleusLogStore
{
    Task SaveLogAsync(NucleusLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<NucleusLog>> GetLogsAsync(DateTime since, CancellationToken cancellationToken = default);
    Task ClearOldLogsAsync(TimeSpan ttl, CancellationToken cancellationToken = default);
}

public class NucleusLogStore(NucleusDbContext db) : INucleusLogStore
{
    public async Task SaveLogAsync(NucleusLog log, CancellationToken cancellationToken = default)
    {
        var connection = db.CreateConnection();
        await db.OpenAsync(connection, cancellationToken);
        
        var sql = @"
        INSERT INTO Nucleus.RequestMetrics (Id, Timestamp, DurationMs, StatusCode, Path)
        VALUES (@Id, @Timestamp, @DurationMs, @StatusCode, @Path);";
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, log, cancellationToken: cancellationToken)
            );

    }

    public Task<IEnumerable<NucleusLog>> GetLogsAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ClearOldLogsAsync(TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}