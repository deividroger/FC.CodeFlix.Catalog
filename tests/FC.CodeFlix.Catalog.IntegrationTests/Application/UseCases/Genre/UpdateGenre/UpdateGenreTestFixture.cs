using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.UpdateGenre;


[CollectionDefinition(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTestFixtureCollection: ICollectionFixture<UpdateGenreTestFixture>
{

}

public class UpdateGenreTestFixture: GenreUseCasesBaseFixture
{
}
