using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Repository = FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CategoryRepository;

[Collection(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTest
{
    private readonly CategoryRepositoryTestFixture _fixture;

    public CategoryRepositoryTest(CategoryRepositoryTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategory = _fixture.GetExampleCategory();

        var categoryRepository = new Repository.CategoryRepository(dbContext);

        await categoryRepository.Insert(exampleCategory, CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var dbCategory = await (_fixture.CreateDbContext(true)).Categories.FindAsync(exampleCategory.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);

    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Get()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategory = _fixture.GetExampleCategory();
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(15);

        exampleCategoriesList.Add(exampleCategory);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(_fixture.CreateDbContext(true));


        var dbCategotory = await categoryRepository.Get(exampleCategory.Id, CancellationToken.None);

        dbCategotory.Should().NotBeNull();
        dbCategotory!.Name.Should().Be(exampleCategory.Name);
        dbCategotory.Id.Should().Be(exampleCategory.Id);
        dbCategotory.Description.Should().Be(exampleCategory.Description);
        dbCategotory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategotory.CreatedAt.Should().Be(exampleCategory.CreatedAt);

    }


    [Fact(DisplayName = nameof(GetThrowIfNotFound))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task GetThrowIfNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleId = Guid.NewGuid();

        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);


        var task = async () => await categoryRepository.Get(exampleId, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>()
             .WithMessage($"category '{exampleId}' not found.");

    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Update()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategory = _fixture.GetExampleCategory();
        var newCategoriesValues = _fixture.GetExampleCategory();
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(15);

        exampleCategoriesList.Add(exampleCategory);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        exampleCategory.Update(newCategoriesValues.Name, newCategoriesValues.Description);



        await categoryRepository.Update(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync();


        var dbCategory = await (_fixture.CreateDbContext(true))
            .Categories.FindAsync(exampleCategory.Id);


        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(newCategoriesValues.Name);
        dbCategory.Description.Should().Be(newCategoriesValues.Description);

        dbCategory.Id.Should().Be(exampleCategory.Id);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);

    }


    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Delete()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategory = _fixture.GetExampleCategory();

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(15);

        exampleCategoriesList.Add(exampleCategory);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        await categoryRepository.Delete(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var dbCategory = await (_fixture.CreateDbContext(true))
            .Categories.FindAsync(exampleCategory.Id);

        dbCategory.Should().BeNull();

    }


    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();


        var exampleCategoriesList = _fixture.GetExampleCategoriesList(15);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        var searhInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var output = await categoryRepository.Search(searhInput, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Items.Should().HaveCount(exampleCategoriesList.Count);

        foreach (Category outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }


    [Fact(DisplayName = nameof(SearchReturnsEmpyWhenPersistenceIsEmpty))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsEmpyWhenPersistenceIsEmpty()
    {
        var dbContext = _fixture.CreateDbContext();

        var categoryRepository = new Repository.CategoryRepository(dbContext);

        var searhInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var output = await categoryRepository.Search(searhInput, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);
 
    }



    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData(10,1,5,5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    
    public async Task SearchReturnsPaginated(int quantityCategoriesToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var dbContext = _fixture.CreateDbContext();


        var exampleCategoriesList = _fixture.GetExampleCategoriesList(quantityCategoriesToGenerate);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        var searhInput = new SearchInput(page, perPage, "", "", SearchOrder.ASC);

        var output = await categoryRepository.Search(searhInput, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
        output.Total.Should().Be(quantityCategoriesToGenerate);
        output.Items.Should().HaveCount(expectedQuantityItems);

        foreach (Category outputItem in output.Items)
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData("action",1, 5, 1,  1)]
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
        var dbContext = _fixture.CreateDbContext();


        var exampleCategoriesList = _fixture.GetExampleCategoriesListWithNames(new List<string>() { 
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

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        var searhInput = new SearchInput(page, perPage, search, "", SearchOrder.ASC);

        var output = await categoryRepository.Search(searhInput, CancellationToken.None);


        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityItemsReturned);

        foreach (Category outputItem in output.Items)
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task SearchOrdered(string orderBy, string order )
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(10);

        await dbContext.AddRangeAsync(exampleCategoriesList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        var searchOrder = order.ToLower() =="asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var searhInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var output = await categoryRepository.Search(searhInput, CancellationToken.None);

        var expectedOrderList = _fixture.CloneCategoriesListOrdered(exampleCategoriesList,orderBy,searchOrder);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
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