using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.DeleteGenre;

[CollectionDefinition(nameof(DeleteGenreTestApiFixture))]
public class DeleteGenreTestApiFixtureCollection: ICollectionFixture<DeleteGenreTestApiFixture> { }

public class DeleteGenreTestApiFixture: GenreBaseFixture
{
}
