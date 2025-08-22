using System.Threading.Channels;
using Nucleus.Core.Models;

namespace Nucleus.Core.Stores;

public sealed class MemoryRequestStore : IRequestStore
{
    private readonly Channel<NucleusLog> _channel;

    public MemoryRequestStore()
    {
        var options = new BoundedChannelOptions(50_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false,
            SingleReader = true
        };

        _channel = Channel.CreateBounded<NucleusLog>(options);
    }

    public async Task Add(NucleusLog log)
    {
        await _channel.Writer.WriteAsync(log);
    }

    // Async stream of logs
    public async Task<IReadOnlyList<NucleusLog>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        var logs = new List<NucleusLog>();

        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var log))
            {
                logs.Add(log);
            }
        }

        return logs.AsReadOnly();
    }
}