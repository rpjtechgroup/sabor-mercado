using SaborMercado.Web.Domain.Sharing;

namespace SaborMercado.Web.Storage;

public interface IPendingShareStore
{
    Task<IReadOnlyList<PendingShare>> GetAllAsync();

    Task EnqueueAsync(PendingShare share);

    Task RemoveAsync(Guid id);
}
