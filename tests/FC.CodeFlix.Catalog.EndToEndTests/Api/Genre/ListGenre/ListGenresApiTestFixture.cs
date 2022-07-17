using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity; 
namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.ListGenre;

[CollectionDefinition(nameof(ListGenresApiTestFixture))]
public class ListGenresApiTestFixtureCollection: ICollectionFixture<ListGenresApiTestFixture>{
}

public class ListGenresApiTestFixture: GenreBaseFixture
{
    public List<DomainEntity.Genre> CloneGenresListOrdered(List<DomainEntity.Genre> genresList, string orderBy, SearchOrder order)
    {
        var listClone = new List<DomainEntity.Genre>(genresList);

        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.ASC) => listClone.OrderBy(x => x.Name)
                .ThenBy(x => x.Id),
            ("name", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Name)
                .ThenByDescending(x => x.Id),

            ("id", SearchOrder.ASC) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Id),

            ("createdat", SearchOrder.ASC) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.DESC) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name)
                   .ThenBy(x => x.Id),
        };

        return orderedEnumerable.ToList();
    }
}
