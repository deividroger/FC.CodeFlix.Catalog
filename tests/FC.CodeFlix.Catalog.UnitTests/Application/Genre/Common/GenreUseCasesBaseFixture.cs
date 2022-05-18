using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.UnitTests.Common;
using Moq;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCasesBaseFixture : BaseFixture
{
    public string GetValidGenreName()
        => Faker.Commerce.Categories(1)[0];


    public DomainEntity.Genre GetExampleGenre(bool? isActive = null)
        => new(GetValidGenreName(), isActive ?? GetRandomBoolean());


    public Mock<IGenreRepository> GetGenreRepositoryMock()
    => new();

    public Mock<IUnitOfWork> GeUnitOfWorkMock()
        => new();

    public Mock<ICategoryRepository> GetCategoryRepositoryMock()
        => new();
}
