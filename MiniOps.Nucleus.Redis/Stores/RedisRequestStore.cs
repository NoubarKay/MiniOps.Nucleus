using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nucleus.Core.Models;
using Nucleus.Core.Stores;
using StackExchange.Redis;

namespace MiniOps.Nucleus.Redis.Stores;

public class RedisRequestStore : IRequestStore
{
    private readonly IDatabase db;
    private const string RedisKey = "nucleus:logs";
    private readonly ILogger<RedisRequestStore> logger;
    
    public RedisRequestStore(string connectionString, ILogger<RedisRequestStore> logger)
    {
        var redis = ConnectionMultiplexer.Connect(connectionString);
        db = redis.GetDatabase();
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        InitializeStreamAndGroupAsync().GetAwaiter().GetResult();
    }
    
    
    public async Task Add(NucleusLog log)
    {
        await db.StreamAddAsync(RedisKey, new NameValueEntry[]
        {
            new("log", JsonSerializer.Serialize(log))
        });
    }
    
    private async Task InitializeStreamAndGroupAsync()
    {
        // Ensure the stream exists
        if (!await db.KeyExistsAsync(RedisKey))
        {
            await db.StreamAddAsync(RedisKey, new NameValueEntry[] { new("log", "init") });
        }

        // Create consumer group if it doesn't exist
        try
        {
            await db.StreamCreateConsumerGroupAsync(RedisKey, "group1", "$"); // start from new messages
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Group already exists, ignore
        }
    }

    public async Task<IReadOnlyList<NucleusLog>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        
        var entries = await db.StreamReadGroupAsync("nucleus:logs", "group1", "consumer1", ">", count: 100000);
        var logs = entries
            .Select(e => JsonSerializer.Deserialize<NucleusLog>(e["log"])!)
            .ToList();

        return logs;
    }
}