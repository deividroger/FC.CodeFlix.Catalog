using FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.Delete;

[CollectionDefinition(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTestFixtureCollection: ICollectionFixture<DeleteGenreTestFixture> {}
public class DeleteGenreTestFixture: GenreUseCasesBaseFixture
{
}
