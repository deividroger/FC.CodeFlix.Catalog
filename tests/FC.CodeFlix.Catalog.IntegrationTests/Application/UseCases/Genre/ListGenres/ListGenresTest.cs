using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;
namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest
{
    private readonly ListGenresTestFixture _fixture;

    public ListGenresTest(ListGenresTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    public async Task ListGenres()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleGenres);

        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var useCase = new UseCase.ListGenres(new GenreRepository(actDbContext), new CategoryRepository(actDbContext));

        var input = new UseCase.ListGenresInput(1, 20);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleGenres.Count);
        output.Items.Should().HaveCount(exampleGenres.Count);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(outputItem.Name);
            outputItem.IsActive.Should().Be(outputItem.IsActive);
        });

    }

    [Fact(DisplayName = nameof(ListGenresReturnsEmptyPersistenceIsEmpty))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    public async Task ListGenresReturnsEmptyPersistenceIsEmpty()
    {
        var arrangeDbContext = _fixture.CreateDbContext();

        var useCase = new UseCase.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

        var input = new UseCase.ListGenresInput(1, 20);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);

    }


    [Fact(DisplayName = nameof(ListGenresVerifyRelations))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    public async Task ListGenresVerifyRelations()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleGenres);

        await arrangeDbContext.AddRangeAsync(exampleCategories);

        var random = new Random();

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(0, 3);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        await arrangeDbContext.AddRangeAsync(genresCategories);

        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var useCase = new UseCase.ListGenres(new GenreRepository(actDbContext), new CategoryRepository(actDbContext));

        var input = new UseCase.ListGenresInput(1, 20);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleGenres.Count);
        output.Items.Should().HaveCount(exampleGenres.Count);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(outputItem.Name);
            outputItem.IsActive.Should().Be(outputItem.IsActive);

            var outputItemCategoryIds = outputItem.
                                            Categories
                                            .Select(x => x.Id)
                                            .ToList();

            outputItemCategoryIds.Should().BeEquivalentTo(exampleItem!.Categories);

            outputItem.Categories.ToList().ForEach(outputCategory =>
           {
               var exampleCategory = exampleCategories.Find(x => x.Id == outputCategory.Id);

               exampleCategory.Should().NotBeNull();
               outputCategory.Name.Should().Be(exampleCategory!.Name);

           });
        });

    }


    [Theory(DisplayName = nameof(ListGenresPaginated))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListGenresPaginated(int quantityToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var exampleGenres = _fixture.GetExampleListGenres(quantityToGenerate);

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleGenres);

        await arrangeDbContext.AddRangeAsync(exampleCategories);

        var random = new Random();

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(0, 3);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        await arrangeDbContext.AddRangeAsync(genresCategories);

        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var useCase = new UseCase.ListGenres(new GenreRepository(actDbContext), new CategoryRepository(actDbContext));

        var input = new UseCase.ListGenresInput(page, perPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleGenres.Count);
        output.Items.Should().HaveCount(expectedQuantityItems);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(outputItem.Name);
            outputItem.IsActive.Should().Be(outputItem.IsActive);

            var outputItemCategoryIds = outputItem.
                                            Categories
                                            .Select(x => x.Id)
                                            .ToList();

            outputItemCategoryIds.Should().BeEquivalentTo(exampleItem!.Categories);

            outputItem.Categories.ToList().ForEach(outputCategory =>
            {
                var exampleCategory = exampleCategories.Find(x => x.Id == outputCategory.Id);

                exampleCategory.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategory!.Name);

            });
        });

    }


    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    [InlineData("action", 1, 5, 1, 1)]
    [InlineData("horror", 1, 5, 3, 3)]
    [InlineData("horror", 2, 5, 0, 3)]
    [InlineData("sci-fi", 1, 5, 4, 4)]
    [InlineData("sci-fi", 1, 2, 2, 4)]
    [InlineData("sci-fi", 2, 3, 1, 4)]
    [InlineData("sci-fi other", 1, 3, 0, 0)]
    [InlineData("robots", 1, 5, 2, 2)]
    public async Task SearchByText(string search,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItemsReturned,
                                             int expectedQuantityTotalItems)
    {
        var exampleGenres = _fixture.GetExampleListGenresByNames(new List<string>() {
           "action",
           "horror",
           "horror - robots",
           "horror - bases on real facts",
           "drama",
           "sci-fi IA",
           "sci-fi Space",
           "sci-fi robots",
           "sci-fi future",
        });

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var arrangeDbContext = _fixture.CreateDbContext();


        await arrangeDbContext.Genres.AddRangeAsync(exampleGenres);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);

        await arrangeDbContext.AddRangeAsync(exampleCategories);

        var random = new Random();

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(0, 3);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        await arrangeDbContext.AddRangeAsync(genresCategories);

        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var useCase = new UseCase.ListGenres(new GenreRepository(actDbContext), new CategoryRepository(actDbContext));

        var input = new UseCase.ListGenresInput(page, perPage, search);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityItemsReturned);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(outputItem.Name);
            outputItem.IsActive.Should().Be(outputItem.IsActive);

            var outputItemCategoryIds = outputItem.
                                            Categories
                                            .Select(x => x.Id)
                                            .ToList();

            outputItemCategoryIds.Should().BeEquivalentTo(exampleItem!.Categories);

            outputItem.Categories.ToList().ForEach(outputCategory =>
            {
                var exampleCategory = exampleCategories.Find(x => x.Id == outputCategory.Id);

                exampleCategory.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategory!.Name);

            });
        });

    }

    [Theory(DisplayName = nameof(Ordered))]
    [Trait("Integration/Application", "ListGenre - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task Ordered(string orderBy, string order)
    {
        
        var exampleGenres = _fixture.GetExampleListGenres(10);
        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var arrangeDbContext = _fixture.CreateDbContext();


        await arrangeDbContext.Genres.AddRangeAsync(exampleGenres);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);

        await arrangeDbContext.AddRangeAsync(exampleCategories);

        var random = new Random();

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(0, 3);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        await arrangeDbContext.AddRangeAsync(genresCategories);

        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var useCase = new UseCase.ListGenres(new GenreRepository(actDbContext), new CategoryRepository(actDbContext));

        var orderEnum = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new UseCase.ListGenresInput(1, 20, "", sort: orderBy, dir: orderEnum);

        
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleGenres.Count);
        output.Items.Should().HaveCount(exampleGenres.Count);

        var expectedOrderList = _fixture.CloneGenresListOrdered(exampleGenres, orderBy, orderEnum);

        for (int i = 0; i < expectedOrderList.Count; i++)
        {
            var expectedItem = expectedOrderList[i];

            var outputItem = output.Items[i];

            expectedItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);

            var outputItemCategoryIds = outputItem.
                                            Categories
                                            .Select(x => x.Id)
                                            .ToList();

            outputItemCategoryIds.Should().BeEquivalentTo(expectedItem!.Categories);

            outputItem.Categories.ToList().ForEach(outputCategory =>
            {
                var exampleCategory = exampleCategories.Find(x => x.Id == outputCategory.Id);

                exampleCategory.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategory!.Name);

            });

        }

    }


}
