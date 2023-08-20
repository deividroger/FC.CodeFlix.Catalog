using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Extensions.Datetime;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.ListGenre;

[Collection(nameof(ListGenresApiTestFixture))]
public class ListGenresApiTest : IDisposable
{
    private readonly ListGenresApiTestFixture _fixture;

    public ListGenresApiTest(ListGenresApiTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(List))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
    public async Task List()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        await _fixture.GenrePersistence.InsertList(exampleGenres);

        var input = new ListGenresInput(1, exampleGenres.Count);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(exampleGenres.Count);
        output.Data!.Count.Should().Be(exampleGenres.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        output.Data.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.IsActive.Should().Be(exampleItem!.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem!.CreatedAt.TrimMilliseconds());

        });
    }


    [Fact(DisplayName = nameof(EmptyWhenThereAreNoItems))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
    public async Task EmptyWhenThereAreNoItems()
    {

        var input = new ListGenresInput(1);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(0);
        output.Data!.Count.Should().Be(0);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

    }


    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(int quantityToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var exampleGenres = _fixture.GetExampleListGenres(quantityToGenerate);

        await _fixture.GenrePersistence.InsertList(exampleGenres);

        var input = new ListGenresInput(page, perPage);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(quantityToGenerate);
        output.Data!.Count.Should().Be(expectedQuantityItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        output.Data.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.IsActive.Should().Be(exampleItem!.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem!.CreatedAt.TrimMilliseconds());

        });
    }


    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
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

        await _fixture.GenrePersistence.InsertList(exampleGenres);

        var input = new ListGenresInput(page, perPage, search);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(expectedQuantityTotalItems);
        output.Data!.Count.Should().Be(expectedQuantityItemsReturned);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        output.Data.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.IsActive.Should().Be(exampleItem!.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem!.CreatedAt.TrimMilliseconds());

        });
    }



    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task ListOrdered(string orderBy, string order)
    {

        var exampleGenres = _fixture.GetExampleListGenres(10);

        await _fixture.GenrePersistence.InsertList(exampleGenres);


        var orderEnum = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListGenresInput(1, 10, dir: orderEnum, sort: orderBy);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(10);
        output.Data!.Count.Should().Be(10);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        var expectedOrderList = _fixture.CloneGenresListOrdered(exampleGenres, orderBy, orderEnum);

        for (int i = 0; i < expectedOrderList.Count; i++)
        {
            
            var expectedItem = expectedOrderList[i];

            var outputItem = output.Data[i];

            expectedItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);

        }
    }


    [Fact(DisplayName = nameof(ListWithRelations))]
    [Trait("EndToEnd/API", "Genre/List - Endpoints")]
    public async Task ListWithRelations()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);

        var random = new Random();

        var exampleCategories = _fixture.GetExampleCategoriesList(15);

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(2, exampleCategories.Count - 1);

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


        await _fixture.GenrePersistence.InsertList(exampleGenres);
        await _fixture.CategoryPersistence.InsertList(exampleCategories);
        await _fixture.GenrePersistence.InsertGenresCategoriesRelationsList(genresCategories);

        var input = new ListGenresInput(1, exampleGenres.Count);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<GenreModelOutput>>($"/genres", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(exampleGenres.Count);
        output.Data!.Count.Should().Be(exampleGenres.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        output.Data.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleGenres.Find(x => x.Id == outputItem.Id);

            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.IsActive.Should().Be(exampleItem!.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem!.CreatedAt.TrimMilliseconds());

           
            outputItem.Categories
                            .ToList()
                            .ForEach(outputrelatedCategory => { 
                                var exampleCategory = exampleCategories.Find(x=>x.Id == outputrelatedCategory.Id);
                                
                                exampleCategory.Should().NotBeNull();
                                outputrelatedCategory.Name.Should().Be(exampleCategory!.Name);
                            });

        });
    }

    public void Dispose() => _fixture.CleanPersistence();

}