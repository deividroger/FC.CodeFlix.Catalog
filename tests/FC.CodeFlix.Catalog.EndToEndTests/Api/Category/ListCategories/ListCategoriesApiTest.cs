using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Extensions.Datetime;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using FC.CodeFlix.Catalog.EndToEndTests.Extensions.Datetime; 
namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.ListCategories;


[Collection(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTest: IDisposable
{
    private readonly ListCategoriesApiTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ListCategoriesApiTest(ListCategoriesApiTestFixture fixture, 
                                 ITestOutputHelper output)
      => (_fixture,_output) = (fixture, output);

    [Fact(DisplayName = nameof(ListCategoriesAndTotalWithDefault))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotalWithDefault()
    {
        var defaultPerPage = 15;
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var (response, output) = await _fixture.ApiClient
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        
        output!.Data.Should().NotBeNull();
        output.Meta.Should().NotBeNull();

        output.Meta!.Total.Should().Be(exampleCategoriesList.Count);
        output.Meta.CurrentPage.Should().Be(1);
        output.Meta.PerPage.Should().Be(defaultPerPage);

        output.Data.Should().HaveCount(defaultPerPage);
        


        foreach (var item in output.Data!)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }
    }


    [Fact(DisplayName = nameof(ItemsEmptyWhenPersistenceEmpty))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ItemsEmptyWhenPersistenceEmpty()
    {
        var (response, output) = await _fixture.ApiClient
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
      
        output!.Meta.Should().NotBeNull();
        output!.Data.Should().HaveCount(0);
        output.Meta!.Total.Should().Be(0);

    }

    [Fact(DisplayName = nameof(ListCategoriesAndTotal))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotal()
    {
        
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);

        var input = new ListCategoriesInput(page:1, perPage: 5);

        var (response, output) = await _fixture.ApiClient
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/",input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Meta.Should().NotBeNull();   

        output.Data.Should().HaveCount(input.PerPage);
        output.Meta!.Total.Should().Be(exampleCategoriesList.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);


        foreach (var item in output.Data!)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
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
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();

        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();


        output!.Data.Should().HaveCount(expectedQuantityItems);
        output.Meta!.Total.Should().Be(exampleCategoriesList.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        foreach (var item in output.Data!)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.TrimMilliseconds()
                .Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
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
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();

        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();


        output!.Data.Should().HaveCount(expectedQuantityItemsReturned);
        output.Meta!.Total.Should().Be(expectedQuantityTotalItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        foreach (var item in output.Data!)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.TrimMilliseconds()
                .Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }

    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("", "asc")]

    public async Task ListOrdered(string orderBy, string order)
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);

        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var inputOrder = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListCategoriesInput(page: 1, perPage: 20,sort: orderBy,dir: inputOrder);

        

        var (response, output) = await _fixture.ApiClient
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();

        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output!.Data.Should().HaveCount(input.PerPage);
        output.Meta!.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);

        output.Data.Should().HaveCount(exampleCategoriesList.Count);

        var expectedOrderList = _fixture.CloneCategoriesListOrdered(exampleCategoriesList, input.Sort, input.Dir);


        for (int i = 0; i < expectedOrderList.Count; i++)
        {

            var outputItem = output.Data![i];
            var expectedItem = expectedOrderList[i];

            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();

            outputItem.Name.Should().Be(expectedItem!.Name);
            outputItem.Id.Should().Be(expectedItem!.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds()
                      .Should().Be(expectedItem.CreatedAt.TrimMilliseconds()) ;
        }
    }


    [Theory(DisplayName = nameof(ListOrderedDates))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
  

    public async Task ListOrderedDates(string orderBy, string order)
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);

        await _fixture.Persistence.InsertList(exampleCategoriesList);


        var inputOrder = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListCategoriesInput(page: 1, perPage: 20, sort: orderBy, dir: inputOrder);



        var (response, output) = await _fixture.ApiClient
                                    .Get<TestApiResponseList<CategoryModelOutput>>($"/categories/", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Meta.Should().NotBeNull();

        
        output!.Data.Should().HaveCount(input.PerPage);
        output.Meta!.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);

        output.Data.Should().HaveCount(exampleCategoriesList.Count);

        DateTime? lastItemDate = null;

        foreach (var item in output.Data!)
        {
            var exampleItem = exampleCategoriesList.FirstOrDefault(x => x.Id == item.Id);

            exampleItem.Should().NotBeNull();

            item.Description.Should().Be(exampleItem!.Description);
            item.Name.Should().Be(exampleItem.Name);
            item.IsActive.Should().Be(exampleItem.IsActive);
            item.CreatedAt.TrimMilliseconds()
                .Should().Be(exampleItem.CreatedAt.TrimMilliseconds());

            if (lastItemDate != null){
                if(order == "asc")
                    Assert.True(item.CreatedAt >= lastItemDate);
                else
                    Assert.True(item.CreatedAt <= lastItemDate);
            }
            lastItemDate = item.CreatedAt;  
        }
    }

    public void Dispose()
     => _fixture.CleanPersistence();
}
