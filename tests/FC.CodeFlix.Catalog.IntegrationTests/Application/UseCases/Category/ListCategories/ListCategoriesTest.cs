using FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ApplicationUseCases = FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[Collection(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTest
{
    private readonly ListCategoriesTestFixture _fixture;

    public ListCategoriesTest(ListCategoriesTestFixture fixture)
       => _fixture = fixture;

    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(15);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(dbContext);

        var input = new ListCategoriesInput(1, 20);

        var useCase = new ApplicationUseCases.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();

        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Items.Should().HaveCount(exampleCategoriesList.Count);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }


    [Fact(DisplayName = nameof(SearchReturnEmptyWhenEmpty))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task SearchReturnEmptyWhenEmpty()
    {
        var dbContext = _fixture.CreateDbContext();

        var categoryRepository = new CategoryRepository(dbContext);

        var input = new ListCategoriesInput(1, 20);

        var useCase = new ApplicationUseCases.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();

        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);

    }


    [Theory(DisplayName = nameof(SearchPaginated))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]

    public async Task SearchPaginated(int quantityCategoriesToGenerate,
                                       int page,
                                       int perPage,
                                       int expectedQuantityItems)
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(quantityCategoriesToGenerate);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(dbContext);

        var input = new ListCategoriesInput(page, perPage);

        var useCase = new ApplicationUseCases.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();

        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Items.Should().HaveCount(expectedQuantityItems);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
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
        var categoryNameList = new List<string>() {
           "action",
           "horror",
           "horror - robots",
           "horror - bases on real facts",
           "drama",
           "sci-fi IA",
           "sci-fi Space",
           "sci-fi robots",
           "sci-fi future",
        };

        var dbContext = _fixture.CreateDbContext();

        var exampleCategoriesList = _fixture.GetExampleCategoriesListWithNames(categoryNameList);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(dbContext);

        var input = new ListCategoriesInput(page,perPage,search);

        var useCase = new ApplicationUseCases.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();

        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityItemsReturned);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }


    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(10);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(dbContext);

        var useCaseOrder  = order == "asc" ?SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListCategoriesInput(1, 20,"",orderBy, useCaseOrder);

        var useCase = new ApplicationUseCases.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);

        var expectedOrderList = _fixture.CloneCategoriesListOrdered(exampleCategoriesList, input.Sort, input.Dir);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();

        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Items.Should().HaveCount(exampleCategoriesList.Count);

        for (int i = 0; i < expectedOrderList.Count; i++)
        {
            var expectedItem = expectedOrderList[i];

            var outputItem = output.Items[i];

            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();

            outputItem.Name.Should().Be(expectedItem!.Name);
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }
}
