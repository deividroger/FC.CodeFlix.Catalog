using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.GetGenre;

[CollectionDefinition(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTestFixtureCollection : ICollectionFixture<GetGenreApiTestFixture>{}


public class GetGenreApiTestFixture: GenreBaseFixture
{
    public GetGenreApiTestFixture():base()
    {

    }
}
