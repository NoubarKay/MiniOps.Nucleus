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
    /// Optional custom table schema name for nucleus tables.
    /// </summary>
    public string SchemaName { get; set; } = "Nucleus";
    
    /// <summary>
    /// Optional seed database on application run.
    /// </summary>
    public bool SeedDatabase { get; set; } = false;
    
    /// <summary>
    /// The table name for storing request logs
    /// </summary>
    public string RequestMetricsTable { get;set; } = "RequestMetrics";
    
    /// <summary>
    /// The table name for storing aggregated metrics.
    /// </summary>
    public string RequestAggregatesTable { get; set; } = "RequestAggregates";
    
    
    /// <summary>
    /// Sets the database type and connection string for Nucleus.
    /// </summary>
    /// <param name="type">The type of database (e.g., SQLServer, PostgreSQL).</param>
    /// <param name="connectionString">The connection string to use for the database.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions UseDatabase(NucleusDatabaseTypes type, string connectionString)
    {
        DatabaseType = type;
        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Specifies the schema name to use for Nucleus tables.
    /// </summary>
    /// <param name="schema">The schema name.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions WithSchema(string schema)
    {
        SchemaName = schema;
        return this;
    }

    /// <summary>
    /// Enables or disables seeding of the database on startup.
    /// </summary>
    /// <param name="seed">If true, the database will be seeded. Default is true.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions EnableSeedDatabase(bool seed = true)
    {
        SeedDatabase = seed;
        return this;
    }

    /// <summary>
    /// Sets the time-to-live (TTL) for request logs in seconds.
    /// Logs older than this value will be deleted automatically.
    /// </summary>
    /// <param name="ttl">TTL in seconds.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions SetLogTtl(int ttl)
    {
        LogTTLSeconds = ttl;
        return this;
    }

    /// <summary>
    /// Sets the interval in seconds for flushing batched logs to the database.
    /// </summary>
    /// <param name="interval">Flush interval in seconds.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions SetBatchFlushInterval(float interval)
    {
        BatchFlushIntervalSeconds = interval;
        return this;
    }
    
    /// <summary>
    /// Sets custom table names for Nucleus.
    /// </summary>
    /// <param name="requestMetricsTable">The table name for storing request logs.</param>
    /// <param name="requestAggregatesTable">The table name for storing aggregated metrics.</param>
    /// <returns>The current <see cref="NucleusOptions"/> instance for fluent configuration.</returns>
    public NucleusOptions WithCustomTables(string requestMetricsTable, string requestAggregatesTable)
    {
        if (!string.IsNullOrWhiteSpace(requestMetricsTable))
            RequestMetricsTable = requestMetricsTable;

        if (!string.IsNullOrWhiteSpace(requestAggregatesTable))
            RequestAggregatesTable = requestAggregatesTable;

        return this;
    }
}