using Nucleus.Core.Config;

namespace Nucleus.Core.Services;

public sealed class SeedService(NucleusDbContext context)
{
    public string GetSeed(NucleusDatabaseTypes dbType) =>
        dbType switch
        {
            NucleusDatabaseTypes.SQLServer => SqlServerSeed,
            NucleusDatabaseTypes.PostGres => PostgresSeed,
            NucleusDatabaseTypes.SQLite => SqliteSeed,
            _ => throw new NotSupportedException($"Database type {dbType} is not supported.")
        };
    
    private string SqlServerSeed = @$"
        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Nucleus')
        BEGIN
            EXEC('CREATE SCHEMA [Nucleus]')
        END;

        -- RequestMetrics Table
        IF NOT EXISTS (SELECT * FROM sys.tables 
                       WHERE name = '{context.Options.RequestMetricsTable}' AND schema_id = SCHEMA_ID('Nucleus'))
        BEGIN
            CREATE TABLE [Nucleus].[{context.Options.RequestMetricsTable}](
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [Timestamp] DATETIME2 NOT NULL,
                [DurationMs] BIGINT NOT NULL,
                [StatusCode] INT NOT NULL,
                [Path] NVARCHAR(2048) NOT NULL
            );

            CREATE NONCLUSTERED INDEX IX_RequestMetrics_Timestamp
            ON [Nucleus].[{context.Options.RequestMetricsTable}]([Timestamp]);
        END;

        -- RequestAggregates Table
        IF NOT EXISTS (SELECT * FROM sys.tables 
                       WHERE name = '{context.Options.RequestAggregatesTable}' AND schema_id = SCHEMA_ID('Nucleus'))
        BEGIN
            CREATE TABLE [Nucleus].[{context.Options.RequestAggregatesTable}](
                BucketTime DATETIME2 NOT NULL PRIMARY KEY,  -- 1 row per second
                TotalRequests INT NOT NULL DEFAULT 0,
                SuccessRequests INT NOT NULL DEFAULT 0,
                FailedRequests INT NOT NULL DEFAULT 0
            );
        END;
        ";

    private const string PostgresSeed = @"
        CREATE SCHEMA IF NOT EXISTS ""Nucleus"";

        CREATE TABLE IF NOT EXISTS ""Nucleus"".""RequestMetrics"" (
            ""Id"" UUID PRIMARY KEY NOT NULL,
            ""Timestamp"" TIMESTAMPTZ NOT NULL,
            ""DurationMs"" BIGINT NOT NULL,
            ""StatusCode"" INT NOT NULL,
            ""Path"" VARCHAR(2048) NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_RequestMetrics_Timestamp
        ON ""Nucleus"".""RequestMetrics""(""Timestamp"");
    ";
    
    private const string SqliteSeed = @"
    CREATE TABLE IF NOT EXISTS RequestMetrics (
        Id TEXT PRIMARY KEY NOT NULL,
        Timestamp DATETIME NOT NULL,
        DurationMs INTEGER NOT NULL,
        StatusCode INTEGER NOT NULL,
        Path TEXT NOT NULL
    );

    CREATE INDEX IF NOT EXISTS IX_RequestMetrics_Timestamp
    ON RequestMetrics(Timestamp);
";
}