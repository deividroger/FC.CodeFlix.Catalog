using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.CreateGenre;


[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection: ICollectionFixture<CreateGenreTestFixture> { }

public class CreateGenreTestFixture: GenreUseCasesBaseFixture
{
    public CreateGenreInput GetExampleInput()
        => new CreateGenreInput(
            GetValidGenreName(),
            GetRandomBoolean()
            );

    public Mock<IGenreRepository> GetGenreRepositoryMock()
        => new();

    public Mock<IUnitOfWork> GeUnitOfWorkMock()
        => new();

    public CreateGenreInput GetExampleInputWithCategories()
    {
        
        var categoriesIds = Enumerable.Range(1, new Random().Next(1, 10))
                                    .Select(_ => Guid.NewGuid()).ToList() ;

       return new CreateGenreInput(
            GetValidGenreName(),
            GetRandomBoolean(),
            categoriesIds
            );
    }

    public Mock<ICategoryRepository> GetCategoryRepositoryMock()
        => new();
}
