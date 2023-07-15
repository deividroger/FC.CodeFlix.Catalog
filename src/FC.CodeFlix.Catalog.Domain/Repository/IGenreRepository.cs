using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;

namespace FC.CodeFlix.Catalog.Domain.Repository;

public interface IGenreRepository : IGenericRepository<Genre>,
                                    ISearchableRepository<Genre>
{
    public Task<IReadOnlyList<Guid>> GetIdsListByIds(List<Guid> ids, CancellationToken cancellationToken);
}
