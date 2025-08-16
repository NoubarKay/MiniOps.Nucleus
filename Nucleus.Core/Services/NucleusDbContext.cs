
using System.Data;
using Dapper;
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
    
    public async Task EnsureNucleusDatabase(string connectionString)
    {
        try{
            var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Nucleus')
            BEGIN
                EXEC('CREATE SCHEMA [Nucleus]')
            END;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestMetrics' AND schema_id = SCHEMA_ID('Nucleus'))
            BEGIN
                CREATE TABLE [Nucleus].[RequestMetrics](
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [Timestamp] DATETIME2 NOT NULL,
                    [DurationMs] BIGINT NOT NULL,
                    [StatusCode] INT NOT NULL,
                    [Path] NVARCHAR(2048) NOT NULL
                );

                -- Add index on Timestamp for faster queries and deletes
                CREATE NONCLUSTERED INDEX IX_RequestMetrics_Timestamp
                ON [Nucleus].[RequestMetrics]([Timestamp]);
            END;
            ";
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(createTableSql);
            Console.WriteLine("Nucleus tables are ready.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while initializing Nucleus database: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Convenience property for TTL in TimeSpan format.
    /// </summary>
    public TimeSpan LogTTL => TimeSpan.FromSeconds(Options.LogTTLSeconds);
}