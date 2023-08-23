using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FluentAssertions;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.DeleteVideo;

[Collection(nameof(VideoBaseFixture))]
public class DeleteVideoApiTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public DeleteVideoApiTest(VideoBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteVideo))]
    [Trait("EndToEnd/API", "Video/DeleteVideo - Endpoints")]
    public async Task DeleteVideo()
    {
        var videoCollection = _fixture.GetVideoCollection(10);

        await _fixture.VideoPersistence.InsertList(videoCollection);

        var mediaCount = await _fixture.VideoPersistence.GetMediaCount();
        var expectedMediaCount = mediaCount - 2;

        var sampleVideo = videoCollection.ElementAt(3);

        var (response, output) = await _fixture.ApiClient
                      .Delete<object>($"/videos/{sampleVideo.Id}");

        response.Should().NotBeNull();
        output.Should().BeNull();
        response!.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var videoFromDb = await _fixture.VideoPersistence.GetById(sampleVideo.Id);
        videoFromDb.Should().BeNull();

        var actualMediaCount = await _fixture.VideoPersistence.GetMediaCount();
        actualMediaCount.Should().Be(expectedMediaCount);

        var allMedias = new[]
        {
            sampleVideo.Trailer!.FilePath,
            sampleVideo.Media!.FilePath,
            sampleVideo.Banner!.Path,
            sampleVideo.Thumb !.Path,
            sampleVideo.ThumbHalf !.Path
        };

        _fixture.WebAppFactory.StorageClient.Verify(x =>
        x.DeleteObjectAsync(It.IsAny<string>(),
                        It.Is<string>(fileName => allMedias.Contains(fileName)),
                        It.IsAny<DeleteObjectOptions>(),
                        It.IsAny<CancellationToken>()),
        Times.Exactly(5));

    }

    [Fact(DisplayName = nameof(DeleteVideoWithRelationship))]
    [Trait("EndToEnd/API", "Video/DeleteVideo - Endpoints")]
    public async Task DeleteVideoWithRelationship()
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

        var mediaCount = await _fixture.VideoPersistence.GetMediaCount();
        var expectedMediaCount = mediaCount - 2;

        var sampleVideo = examplesVideos.ElementAt(5);

        var (response, output) = await _fixture.ApiClient
                      .Delete<object>($"/videos/{sampleVideo.Id}");

        response.Should().NotBeNull();
        output.Should().BeNull();
        response!.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var videoFromDb = await _fixture.VideoPersistence.GetById(sampleVideo.Id);
        videoFromDb.Should().BeNull();

        var categoriesFromDb = await _fixture.VideoPersistence.GetVideosCategories(sampleVideo.Id);
        categoriesFromDb.Should().BeNullOrEmpty();

        var genreFromDb = await _fixture.VideoPersistence.GetVideosGenres(sampleVideo.Id);
        genreFromDb.Should().BeNullOrEmpty();

        var castMembers = await _fixture.VideoPersistence.GetVideosCastMembers(sampleVideo.Id);
        castMembers.Should().BeNullOrEmpty();

        var actualMediaCount = await _fixture.VideoPersistence.GetMediaCount();
        actualMediaCount.Should().Be(expectedMediaCount);


        var allMedias = new[]
        {
            sampleVideo.Trailer!.FilePath,
            sampleVideo.Media!.FilePath,
            sampleVideo.Banner!.Path,
            sampleVideo.Thumb !.Path,
            sampleVideo.ThumbHalf !.Path
        };

        _fixture.WebAppFactory.StorageClient.Verify(x =>
        x.DeleteObjectAsync(It.IsAny<string>(),
                        It.Is<string>(fileName => allMedias.Contains(fileName)),
                        It.IsAny<DeleteObjectOptions>(),
                        It.IsAny<CancellationToken>()),
        Times.Exactly(5));

    }


    [Fact(DisplayName = nameof(Error404WhenVideoIdNotFound))]
    [Trait("EndToEnd/API", "Video/DeleteVideo - Endpoints")]
    public async Task Error404WhenVideoIdNotFound()
    {
        var randomId = Guid.NewGuid();

        var (response, output) = await _fixture.ApiClient
                      .Delete<ProblemDetails>($"/videos/{randomId}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        output!.Title.Should().Be("Not Found");
        output.Detail.Should().Be($"Video '{randomId}' not found.");

    }


    public void Dispose()
        => _fixture.CleanPersistence();
}
