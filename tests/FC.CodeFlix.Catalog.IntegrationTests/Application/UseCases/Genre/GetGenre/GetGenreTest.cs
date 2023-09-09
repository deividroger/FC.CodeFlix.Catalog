using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.GetGenre;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.GetGenre
{
    [Collection(nameof(GetGenreTestFixture))]
    public class GetGenreTest
    {
        private readonly GetGenreTestFixture _fixture;

        public GetGenreTest(GetGenreTestFixture fixture)
            => _fixture = fixture;

        [Fact(DisplayName = nameof(GetGenre))]
        [Trait("Integration/Application", "GetGenre - Use cases")]
        public async Task GetGenre()
        {
            var genresExamplesList = _fixture.GetExampleListGenres(10);

            var expectedGenre = genresExamplesList[5];
            var dbContext = _fixture.CreateDbContext();
            
            await dbContext.AddRangeAsync(genresExamplesList);
            dbContext.SaveChanges();

            var dbActContext = _fixture.CreateDbContext(true);  
            var categoryRepository = new CategoryRepository(dbActContext);
            var repository = new GenreRepository(dbActContext);

            var input = new UseCase.GetGenreInput(expectedGenre.Id);

            var useCase = new UseCase.GetGenre(repository, categoryRepository);

            var output = await useCase.Handle(input, CancellationToken.None);

            output.Should().NotBeNull();
            output.Name.Should().Be(expectedGenre.Name);

            output.IsActive.Should().Be(expectedGenre.IsActive);
            output.Id.Should().Be(expectedGenre.Id);
            output.CreatedAt.Should().Be(expectedGenre.CreatedAt);
        }


        [Fact(DisplayName = nameof(GetThrowsWhenNotFound))]
        [Trait("Integration/Application", "GetGenre - Use cases")]
        public async Task GetThrowsWhenNotFound()
        {
            var genresExamplesList = _fixture.GetExampleListGenres(10);
            var randomGuid = Guid.NewGuid();

            var dbContext = _fixture.CreateDbContext();
            await dbContext.AddRangeAsync(genresExamplesList);
            dbContext.SaveChanges();

            var dbActContext = _fixture.CreateDbContext(true);
            var categoryRepository = new CategoryRepository(dbActContext);
            var repository = new GenreRepository(dbActContext);
            var useCase = new UseCase.GetGenre(repository, categoryRepository);

            var input = new UseCase.GetGenreInput(randomGuid);

            

            var output = async () => await useCase.Handle(input, CancellationToken.None);

            await output.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{randomGuid}' not found.");

        }


        [Fact(DisplayName = nameof(GeGetGenreWithCategoryRelationstGenre))]
        [Trait("Integration/Application", "GetGenre - Use cases")]
        public async Task GeGetGenreWithCategoryRelationstGenre()
        {
            var categoriesExampleList = _fixture.GetExampleCategoriesList(5);
            var genresExamplesList = _fixture.GetExampleListGenres(10);

            var expectedGenre = genresExamplesList[5];
            categoriesExampleList.ForEach(category => expectedGenre.AddCategory(category.Id));


            var dbContext = _fixture.CreateDbContext();
            await dbContext.AddRangeAsync(categoriesExampleList);
            await dbContext.AddRangeAsync(genresExamplesList);
            await dbContext.GenresCategories.AddRangeAsync(
                expectedGenre.Categories
                             .Select(categoryId => new GenresCategories(categoryId, expectedGenre.Id)));

            dbContext.SaveChanges();

            var dbActContext = _fixture.CreateDbContext(true);
            var categoryRepository = new CategoryRepository(dbActContext);
            var repository = new GenreRepository(dbActContext);
            var useCase = new UseCase.GetGenre(repository, categoryRepository);

            var input = new UseCase.GetGenreInput(expectedGenre.Id);


            var output = await useCase.Handle(input, CancellationToken.None);

            output.Should().NotBeNull();
            output.Name.Should().Be(expectedGenre.Name);

            output.IsActive.Should().Be(expectedGenre.IsActive);
            output.Id.Should().Be(expectedGenre.Id);
            output.CreatedAt.Should().Be(expectedGenre.CreatedAt);

            output.Categories.Should().HaveCount(expectedGenre.Categories.Count);


            output.Categories.ToList().ForEach(relationModel =>
            {
                expectedGenre.Categories.Should().Contain(relationModel.Id);

                var relatedCategory = categoriesExampleList.Find(category => category.Id == relationModel.Id);

                relationModel.Name.Should().Be(relatedCategory!.Name);

            });

        }
    }
}