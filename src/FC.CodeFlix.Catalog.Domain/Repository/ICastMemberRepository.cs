using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;

namespace FC.CodeFlix.Catalog.Domain.Repository;

public interface ICastMemberRepository : IGenericRepository<CastMember>, ISearchableRepository<CastMember>
{
    public Task<IReadOnlyList<Guid>> GetIdsListByIds(List<Guid> ids, CancellationToken cancellationToken);
}
