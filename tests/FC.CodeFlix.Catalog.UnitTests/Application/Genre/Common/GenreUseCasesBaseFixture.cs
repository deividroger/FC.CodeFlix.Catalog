using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.UnitTests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCasesBaseFixture : BaseFixture
{

    public List<Guid> GetRandomIdsList(int? count = null)
        => Enumerable
            .Range(1, count ?? (new Random().Next(1, 10)))
            .Select(_ => Guid.NewGuid())
            .ToList();

    public DomainEntity.Genre GetExampleGenre(bool? isActive = null, List<Guid>? categoriesIds = null)
    {
        var genre = new DomainEntity.Genre(GetValidGenreName(), isActive ?? GetRandomBoolean());

        categoriesIds?.ForEach(genre.AddCategory);

        return genre;
    }

    public string GetValidGenreName()
        => Faker.Commerce.Categories(1)[0];

    public List<DomainEntity.Genre> GetExampleGenresList(int count = 10)
          => Enumerable.Range(1, count).Select(_ =>
          {
              var genre = GetExampleGenre();

              GetRandomIdsList().ForEach(genre.AddCategory);
              return genre;
          }).ToList();

    public Mock<IGenreRepository> GetGenreRepositoryMock()
    => new();

    public Mock<IUnitOfWork> GeUnitOfWorkMock()
        => new();

    public Mock<ICategoryRepository> GetCategoryRepositoryMock()
        => new();
}
