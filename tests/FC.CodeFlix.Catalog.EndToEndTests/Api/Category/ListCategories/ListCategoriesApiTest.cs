using FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.ListCategories;

[Collection(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTest: IDisposable
{
    private readonly ListCategoriesApiTestFixture _fixture;

    public ListCategoriesApiTest(ListCategoriesApiTestFixture fixture)
      => _fixture = fixture;

    [Fact(DisplayName = nameof(ListCategoriesAndTotalWithDefault))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotalWithDefault()
    {
        var defaultPerPage = 15;
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(defaultPerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Page.Should().Be(1);
        output.PerPage.Should().Be(defaultPerPage);

        foreach (var item in output.Items)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }


    [Fact(DisplayName = nameof(ItemsEmptyWhenPersistenceEmpty))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ItemsEmptyWhenPersistenceEmpty()
    {
        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(0);
        output.Total.Should().Be(0);

    }



    [Fact(DisplayName = nameof(ListCategoriesAndTotal))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotal()
    {
        
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var input = new ListCategoriesInput(page:1, perPage: 5);

        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/",input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        foreach (var item in output.Items)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }


    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(int quantityCategoriesToGenerate,
                                       int page,
                                       int perPage,
                                       int expectedQuantityItems)
    {

        var exampleCategoriesList = _fixture.GetExampleCategoriesList(quantityCategoriesToGenerate);
        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var input = new ListCategoriesInput(page, perPage);

        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(expectedQuantityItems);
        output.Total.Should().Be(exampleCategoriesList.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        foreach (var item in output.Items)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
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

        var exampleCategoriesList = _fixture.GetExampleCategoriesListWithNames(categoryNameList);
        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var input = new ListCategoriesInput(page, perPage,search);

        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(expectedQuantityItemsReturned);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        foreach (var item in output.Items)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }

    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task ListOrdered(string orderBy, string order)
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);

        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var inputOrder = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListCategoriesInput(page: 1, perPage: 20,sort: orderBy,dir: inputOrder);

        

        var (response, output) = await _fixture.ApiClient
                                    .Get<ListCategoriesOutput>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(input.PerPage);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);

        output.Items.Should().HaveCount(exampleCategoriesList.Count);

        var expectedOrderList = _fixture.CloneCategoriesListOrdered(exampleCategoriesList, input.Sort, input.Dir);

        for (int i = 0; i < expectedOrderList.Count; i++)
        {

            var outputItem = output.Items[i];
            var expectedItem = expectedOrderList[i];

            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();

            outputItem.Name.Should().Be(expectedItem!.Name);
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }

    public void Dispose()
     => _fixture.CleanPersistence();
}
