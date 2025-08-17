namespace Nucleus.Core.Config;

/// <summary>
/// Central configuration object for MiniOps.
/// Holds DB info, TTL, simulation settings, and other runtime options.
/// </summary>
public sealed class NucleusOptions
{
    /// <summary>
    /// The type of database MiniOps will use.
    /// Supported: SqlServer, PostgreSQL, MySQL, SQLite
    /// </summary>
    public NucleusDatabaseTypes DatabaseType { get; set; }

    /// <summary>
    /// The connection string for the selected database.
    /// </summary>
    public string ConnectionString { get; set; } = "";
    
    /// <summary>
    /// Time-to-live for request logs, in seconds.
    /// Expired logs will be deleted automatically by the background cleanup service.
    /// </summary>
    public int LogTTLSeconds { get; set; } = 1;
    
    /// <summary>
    /// How often, in seconds, the background flush service writes accumulated logs to the database.
    /// </summary>
    public float BatchFlushIntervalSeconds { get; set; } = 1;
    
    /// <summary>
    /// Optional custom table name for request logs.
    /// </summary>
    public string SchemaName { get; set; } = "Nucleus";
    
    /// <summary>
    /// Optional custom table name for request logs.
    /// </summary>
    public bool SeedDatabase { get; set; } = false;
}