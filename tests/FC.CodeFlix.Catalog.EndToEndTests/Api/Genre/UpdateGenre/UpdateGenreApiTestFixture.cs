using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.UpdateGenre;

[CollectionDefinition(nameof(UpdateGenreApiTestFixture))]
public class UpdateGenreApiTestFixtureCollection: 
       ICollectionFixture<UpdateGenreApiTestFixture>
{

}

public class UpdateGenreApiTestFixture: GenreBaseFixture
{
}
