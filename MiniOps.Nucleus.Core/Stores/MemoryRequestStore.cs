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
    public IAsyncEnumerable<NucleusLog> ReadAllAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}