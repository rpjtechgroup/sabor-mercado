using SaborMercado.Web.Domain.Sharing;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryPendingShareStore : IPendingShareStore
{
    private readonly List<PendingShare> _items = [];

    public Task<IReadOnlyList<PendingShare>> GetAllAsync() => Task.FromResult<IReadOnlyList<PendingShare>>(_items);

    public Task EnqueueAsync(PendingShare share)
    {
        _items.Add(share);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id)
    {
        _items.RemoveAll(s => s.Id == id);
        return Task.CompletedTask;
    }
}
