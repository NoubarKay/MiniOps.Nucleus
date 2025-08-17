
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
    
    public async Task EnsureNucleusDatabase(string connectionString, string sql)
    {
        try{
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(sql);
            
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(sql);
            Console.WriteLine("Nucleus tables are ready.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while initializing Nucleus database: {ex.Message}");
            throw;
        }
    }

    public NucleusOptions GetConfig()
    {
        return Options;
    }
}