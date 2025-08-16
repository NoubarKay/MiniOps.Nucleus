
using System.Data;
using Nucleus.Core.Config;
using Microsoft.Data.SqlClient;
namespace Nucleus.Core.Services;

/// <summary>
/// The central context for MiniOps. Holds configuration and provides DB connections.
/// Singleton instance.
/// </summary>
public class NucleusDbContext(NucleusOptions options)
{
    public NucleusOptions Options { get; } = options ?? throw new ArgumentNullException(nameof(options));
    
    /// <summary>
    /// Creates a new IDbConnection based on the DatabaseType in Options.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(Options.ConnectionString))
            throw new InvalidOperationException("MiniOpsOptions.ConnectionString is not set.");

        return Options.DatabaseType switch
        {
            NucleusDatabaseTypes.SQLServer  => new SqlConnection(Options.ConnectionString),
            _ => throw new NotSupportedException($"Database type '{Options.DatabaseType}' is not supported.")
        };
    }

    public async Task<IDbConnection> OpenAsync(IDbConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection is SqlConnection sqlConn)
        {
            await sqlConn.OpenAsync(cancellationToken);
        }
        else
        {
            connection.Open(); // synchronous fallback
        }

        return connection;
    }
    
    /// <summary>
    /// Convenience property for TTL in TimeSpan format.
    /// </summary>
    public TimeSpan LogTTL => TimeSpan.FromSeconds(Options.LogTTLSeconds);
}