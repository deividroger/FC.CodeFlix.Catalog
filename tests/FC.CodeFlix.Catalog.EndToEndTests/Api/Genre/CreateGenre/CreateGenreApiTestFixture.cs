using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.CreateGenre;


[CollectionDefinition(nameof(CreateGenreApiTestFixture))]
public class CreateGenreApiTestFixtureCollection: ICollectionFixture<CreateGenreApiTestFixture> { }

public class CreateGenreApiTestFixture: GenreBaseFixture
{
}
