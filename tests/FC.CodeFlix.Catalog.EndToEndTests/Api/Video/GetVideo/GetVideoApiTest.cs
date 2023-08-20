using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.GetVideo;

[Collection(nameof(VideoBaseFixture))]
public class GetVideoApiTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public GetVideoApiTest(VideoBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(GetVideo))]
    [Trait("EndToEnd/API", "Video/GetVideo - Endpoints")]
    public async Task GetVideo()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(3);
        var exampleGenre = _fixture.GetExampleListGenres(4);
        var exampleCastMembers = _fixture.GetExampleCastMemberList(5);

        var examplesVideos = _fixture.GetVideoCollection(10);

        examplesVideos.ForEach(video =>
        {
            exampleCategories.ForEach(category => video.AddCategory(category.Id));
            exampleGenre.ForEach(genre => video.AddGenre(genre.Id));
            exampleCastMembers.ForEach(castMember => video.AddCastMember(castMember.Id));
        });


        await _fixture.CategoryPersistence.InsertList(exampleCategories);
        await _fixture.GenrePersistence.InsertList(exampleGenre);
        await _fixture.CastMemberPersistence.InsertList(exampleCastMembers);

        await _fixture.VideoPersistence.InsertList(examplesVideos);

        var exampleItem = examplesVideos.ElementAt(5);


        var (response, output) = await _fixture.ApiClient
                      .Get<TestApiResponse<VideoModelOutput>>($"/videos/{exampleItem.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();

        output.Data!.Title.Should().Be(exampleItem!.Title);
        output.Data.Description.Should().Be(exampleItem.Description);
        output.Data.YearLaunched.Should().Be(exampleItem.YearLaunched);
        output.Data.Opened.Should().Be(exampleItem.Opened);
        output.Data.Published.Should().Be(exampleItem.Published);
        output.Data.Duration.Should().Be(exampleItem.Duration);
        output.Data.Rating.Should().Be(exampleItem.Rating.ToStringSignal());

        var expectedCategories = exampleCategories.Select(category => new VideoModelOutputRelatedAggregate(category.Id, null));
        output.Data.Categories.Should().BeEquivalentTo(expectedCategories);

        var expectedGenres = exampleGenre.Select(genre => new VideoModelOutputRelatedAggregate(genre.Id, null));
        output.Data.Genres.Should().BeEquivalentTo(expectedGenres);

        var expectedCastMembers = exampleCastMembers.Select(castMember => new VideoModelOutputRelatedAggregate(castMember.Id, null));
        output.Data.CastMembers.Should().BeEquivalentTo(expectedCastMembers);

    }


    [Fact(DisplayName = nameof(Error404WhenIdNotFound))]
    [Trait("EndToEnd/API", "Video/GetVideo - Endpoints")]
    public async Task Error404WhenIdNotFound()
    {
        var randomId = Guid.NewGuid();

        var (response, output) = await _fixture.ApiClient
                     .Get<ProblemDetails>($"/videos/{randomId}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);

        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found"); 
        output.Detail.Should().Be($"Video '{randomId}' not found.");
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}
