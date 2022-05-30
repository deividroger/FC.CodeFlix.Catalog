using FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.UpdateGenre;

[CollectionDefinition(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTestFixtureCollection: ICollectionFixture<UpdateGenreTestFixture> { }

public class UpdateGenreTestFixture: GenreUseCasesBaseFixture
{
}
