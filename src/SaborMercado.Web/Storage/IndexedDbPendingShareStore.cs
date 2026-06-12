using SaborMercado.Web.Domain.Sharing;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbPendingShareStore(IndexedDbInterop indexedDb) : IPendingShareStore
{
    public async Task<IReadOnlyList<PendingShare>> GetAllAsync() =>
        await indexedDb.GetAllAsync<PendingShare>(StorageSchema.PendingSharesStore);

    public Task EnqueueAsync(PendingShare share) =>
        indexedDb.PutAsync(StorageSchema.PendingSharesStore, share).AsTask();

    public Task RemoveAsync(Guid id) =>
        indexedDb.DeleteAsync(StorageSchema.PendingSharesStore, id).AsTask();
}
