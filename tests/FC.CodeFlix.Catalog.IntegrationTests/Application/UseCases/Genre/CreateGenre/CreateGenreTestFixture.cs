using FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.CreateGenre;


[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection: ICollectionFixture<CreateGenreTestFixture>
{

}

public class CreateGenreTestFixture : GenreUseCasesBaseFixture
{
    public CreateGenreInput GetExampleInput()
        => new (GetValidGenreName(),GetRandomBoolean());
}
