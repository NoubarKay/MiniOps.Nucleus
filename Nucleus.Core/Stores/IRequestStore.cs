using System.Collections.Concurrent;
using Nucleus.Core.Models;

namespace Nucleus.Core.Stores;

public interface IRequestStore
{
    void Add(NucleusLog log);
    List<NucleusLog> Flush();
}

public class RequestStore : IRequestStore
{
    private readonly ConcurrentQueue<NucleusLog> _queue = new();

    public void Add(NucleusLog log) => _queue.Enqueue(log);

    public List<NucleusLog> Flush()
    {
        var list = new List<NucleusLog>();
        while (_queue.TryDequeue(out var log))
        {
            list.Add(log);
        }
        return list;
    }
}