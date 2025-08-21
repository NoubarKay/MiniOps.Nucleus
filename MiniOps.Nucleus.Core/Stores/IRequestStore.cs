using Nucleus.Core.Models;

namespace Nucleus.Core.Stores;

public interface IRequestStore
{
    Task Add(NucleusLog log);

    IAsyncEnumerable<NucleusLog> ReadAllAsync(CancellationToken cancellationToken = default);
}