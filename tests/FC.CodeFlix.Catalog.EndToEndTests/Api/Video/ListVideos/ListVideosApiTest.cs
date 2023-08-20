using FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.ListVideos;


[Collection(nameof(VideoBaseFixture))]
public class ListVideosApiTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public ListVideosApiTest(VideoBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(ListVideos))]
    [Trait("EndToEnd/API", "Video/ListVideos - Endpoints")]
    public async Task ListVideos()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(3);
        var exampleGenre = _fixture.GetExampleListGenres(4);
        var exampleCastMembers = _fixture.GetExampleCastMemberList(5);

        var examplesVideos = _fixture.GetVideoCollection(10);

        examplesVideos.ForEach(video => {
            exampleCategories.ForEach(category => video.AddCategory(category.Id));
            exampleGenre.ForEach(genre => video.AddGenre(genre.Id));
            exampleCastMembers.ForEach(castMember => video.AddCastMember(castMember.Id));
        });

        
        await _fixture.CategoryPersistence.InsertList(exampleCategories);
        await _fixture.GenrePersistence.InsertList(exampleGenre);
        await _fixture.CastMemberPersistence.InsertList(exampleCastMembers);

        await _fixture.VideoPersistence.InsertList(examplesVideos);

        var input = new ListVideosInput();
        
        var(response, output) = await _fixture.ApiClient
                      .Get<TestApiResponseList<VideoModelOutput>>($"/videos",input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        
        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Meta!.CurrentPage.Should().Be(input.Page);
        output.Meta!.PerPage.Should().Be(input.PerPage);
        

        output!.Data.Should().NotBeNull();
        output.Data!.Count.Should().Be(examplesVideos.Count);



        output.Data.ForEach(outputItem =>
        {
            var exampleItem = examplesVideos.Find(example => example.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Title.Should().Be(exampleItem!.Title);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.YearLaunched.Should().Be(exampleItem.YearLaunched);
            outputItem.Opened.Should().Be(exampleItem.Opened);
            outputItem.Published.Should().Be(exampleItem.Published);
            outputItem.Duration.Should().Be(exampleItem.Duration);
            outputItem.Rating.Should().Be(exampleItem.Rating.ToStringSignal());

            var expectedCategories =exampleCategories.Select(category => new VideoModelOutputRelatedAggregate(category.Id, category.Name));
            outputItem.Categories.Should().BeEquivalentTo(expectedCategories);

            var expectedGenres = exampleGenre.Select(genre => new VideoModelOutputRelatedAggregate(genre.Id, genre.Name));
            outputItem.Genres.Should().BeEquivalentTo(expectedGenres);

            var expectedCastMembers = exampleCastMembers.Select(castMember => new VideoModelOutputRelatedAggregate(castMember.Id, castMember.Name));
            outputItem.CastMembers.Should().BeEquivalentTo(expectedCastMembers);

        });

    }


    [Fact(DisplayName = nameof(ReturnsEmptyWhenThereIsVideo))]
    [Trait("EndToEnd/API", "Video/ListVideos - Endpoints")]
    public async Task ReturnsEmptyWhenThereIsVideo()
    {
        var input = new ListVideosInput();

        var (response, output) = await _fixture.ApiClient
                      .Get<TestApiResponseList<VideoModelOutput>>($"/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Meta!.CurrentPage.Should().Be(input.Page);
        output.Meta!.PerPage.Should().Be(input.PerPage);
        output.Meta!.Total.Should().Be(0);

        output!.Data.Should().NotBeNull();
        output.Data!.Should().BeEmpty();

    }

    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/API", "Video/ListVideos - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(int quantityToGenerate,
                                            int page,
                                            int perPage,
                                            int expectedQuantityItems)
    {
        var exampleVideos = _fixture.GetVideoCollection(quantityToGenerate);

        await _fixture.VideoPersistence.InsertList(exampleVideos);

        var input = new ListVideosInput(page, perPage);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<VideoModelOutput>>($"/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(quantityToGenerate);
        output.Data!.Count.Should().Be(expectedQuantityItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        output.Data.ForEach(outputItem =>
        {
            var exampleItem = exampleVideos.Find(example => example.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Title.Should().Be(exampleItem!.Title);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.YearLaunched.Should().Be(exampleItem.YearLaunched);
            outputItem.Opened.Should().Be(exampleItem.Opened);
            outputItem.Published.Should().Be(exampleItem.Published);
            outputItem.Duration.Should().Be(exampleItem.Duration);
            outputItem.Rating.Should().Be(exampleItem.Rating.ToStringSignal());

        });
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/API", "Video/ListVideos - Endpoints")]
    [InlineData("title", "asc")]
    [InlineData("title", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task ListOrdered(string orderBy, string order)
    {

        var examplesVideos = _fixture.GetVideoCollection(10);

        await _fixture.VideoPersistence.InsertList(examplesVideos);


        var orderEnum = order == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new ListGenresInput(1, 10, dir: orderEnum, sort: orderBy);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<VideoModelOutput>>($"/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(10);
        output.Data!.Count.Should().Be(10);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        var expectedOrderList = _fixture.CloneVideoListOrdered(examplesVideos, orderBy, orderEnum);

        output.Data.Should().Equal(expectedOrderList, (v1, v2) => v1.Id == v2.Id);
    }


    [Theory(DisplayName = nameof(SearchVideo))]
    [Trait("EndToEnd/API", "Video/ListVideos - Endpoints")]
    [InlineData("007",1,2,2,4)]
    [InlineData("st", 2, 2, 1, 3)]
    [InlineData("007: Casino", 1, 2, 1, 1)]
    [InlineData("terminator", 1, 5, 0, 0)]
    public async Task SearchVideo(string searchTerm,
                                           int page,
                                           int perPage,
                                           int expectedReturnedItems,
                                           int expectedTotalItems)
    {
        var moviesNames = new[]
        {
            "007: Dr. No",
            "007: Casino Royale",
            "007: GoldFinger",
            "007: Skyfall",
            "Star Wars: Return of the Jedi",
            "Star Wars: The Empire Strikes Back",
            "Interstellar"
        };

        var exampleVideos = _fixture.GetVideoCollection(moviesNames);

        await _fixture.VideoPersistence.InsertList(exampleVideos);

        var input = new ListVideosInput(page, perPage,searchTerm);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<VideoModelOutput>>($"/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(expectedTotalItems);
        output.Data!.Count.Should().Be(expectedReturnedItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);

        if (output.Data.Any())
        {
            output.Data.Should().AllSatisfy(video =>
            video.Title.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}
