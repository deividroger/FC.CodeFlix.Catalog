namespace FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;

public interface ISearchableRepository<TAgrregate>
    where TAgrregate : AggregateRoot
{
    Task<SearchOutput<TAgrregate>> Search(SearchInput input,CancellationToken cancellationToken);
}
