using FC.CodeFlix.Catalog.Api.ApiModels.Video;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.UpdateVideo;

[Collection(nameof(VideoBaseFixture))]
public class UpdateVideoApiTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public UpdateVideoApiTest(VideoBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(UpdateVideo))]
    [Trait("EndToEnd/API", "Video/UpdateVideo - Endpoints")]
    public async Task UpdateVideo()
    {
        var videos = _fixture.GetVideoCollection();

        await _fixture.VideoPersistence.InsertList(videos);

        var targetVideoId = videos.ElementAt(5).Id;

        var updateVideoApiInput = new UpdateVideoApiInput()
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            YearLaunched = _fixture.GetValidYearLaunched(),
            Duration = _fixture.GetValidDuration(),
            Opened = _fixture.GetRandomBoolean(),
            Published = _fixture.GetRandomBoolean(),
            Rating = _fixture.GetRandomRating().ToStringSignal()
        };

        var (response, output) = await _fixture.ApiClient
            .Put<TestApiResponse<VideoModelOutput>>($"/videos/{targetVideoId}", updateVideoApiInput);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();

        output.Data!.Id.Should().Be(targetVideoId);
        output.Data!.Title.Should().Be(updateVideoApiInput!.Title);
        output.Data.Description.Should().Be(updateVideoApiInput.Description);
        output.Data.YearLaunched.Should().Be(updateVideoApiInput.YearLaunched);
        output.Data.Opened.Should().Be(updateVideoApiInput.Opened);
        output.Data.Published.Should().Be(updateVideoApiInput.Published);
        output.Data.Duration.Should().Be(updateVideoApiInput.Duration);
        output.Data.Rating.Should().Be(updateVideoApiInput.Rating);



        var videoFromDb = await _fixture.VideoPersistence.GetById(targetVideoId);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Id.Should().Be(targetVideoId);
        videoFromDb.Title.Should().Be(updateVideoApiInput.Title);
        videoFromDb.Description.Should().Be(updateVideoApiInput.Description);
        videoFromDb.YearLaunched.Should().Be(updateVideoApiInput.YearLaunched);
        videoFromDb.Opened.Should().Be(updateVideoApiInput.Opened);
        videoFromDb.Published.Should().Be(updateVideoApiInput.Published);
        videoFromDb.Duration.Should().Be(updateVideoApiInput.Duration);
        videoFromDb.Rating.Should().Be(updateVideoApiInput.Rating!.ToRating());

    }

    [Fact(DisplayName = nameof(UpdateVideoWithRelationships))]
    [Trait("EndToEnd/API", "Video/UpdateVideo - Endpoints")]
    public async Task UpdateVideoWithRelationships()
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

        var targetVideoId = examplesVideos.ElementAt(5).Id;

        var targetCategories = new[]
        {
            exampleCategories.ElementAt(1),
        };

        var targetGenres = new[]
        {
            exampleGenre.ElementAt(0),
            exampleGenre.ElementAt(2),
        };

        var targetCastMembers = new[]
        {
            exampleCastMembers.ElementAt(1),
            exampleCastMembers.ElementAt(2),
            exampleCastMembers.ElementAt(3),
        };



        var updateVideoApiInput = new UpdateVideoApiInput()
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            YearLaunched = _fixture.GetValidYearLaunched(),
            Duration = _fixture.GetValidDuration(),
            Opened = _fixture.GetRandomBoolean(),
            Published = _fixture.GetRandomBoolean(),
            Rating = _fixture.GetRandomRating().ToStringSignal(),
            CategoriesIds = targetCategories.Select(category => category.Id).ToList(),
            GenresIds = targetGenres.Select(genre => genre.Id).ToList(),
            CastMembersIds = targetCastMembers.Select(castMember => castMember.Id).ToList(),
        };

        var (response, output) = await _fixture.ApiClient
            .Put<TestApiResponse<VideoModelOutput>>($"/videos/{targetVideoId}", updateVideoApiInput);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();

        output.Data!.Id.Should().Be(targetVideoId);
        output.Data!.Title.Should().Be(updateVideoApiInput!.Title);
        output.Data.Description.Should().Be(updateVideoApiInput.Description);
        output.Data.YearLaunched.Should().Be(updateVideoApiInput.YearLaunched);
        output.Data.Opened.Should().Be(updateVideoApiInput.Opened);
        output.Data.Published.Should().Be(updateVideoApiInput.Published);
        output.Data.Duration.Should().Be(updateVideoApiInput.Duration);
        output.Data.Rating.Should().Be(updateVideoApiInput.Rating);

        var expectedCategories = targetCategories.Select(category => new VideoModelOutputRelatedAggregate(category.Id));
        output.Data.Categories.Should().BeEquivalentTo(expectedCategories);

        var expectedGenres = targetGenres.Select(genre => new VideoModelOutputRelatedAggregate(genre.Id));
        output.Data.Genres.Should().BeEquivalentTo(expectedGenres);

        var expectedCastMembers = targetCastMembers.Select(castMember => new VideoModelOutputRelatedAggregate(castMember.Id));
        output.Data.CastMembers.Should().BeEquivalentTo(expectedCastMembers);


        var videoFromDb = await _fixture.VideoPersistence.GetById(targetVideoId);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Id.Should().Be(targetVideoId);
        videoFromDb.Title.Should().Be(updateVideoApiInput.Title);
        videoFromDb.Description.Should().Be(updateVideoApiInput.Description);
        videoFromDb.YearLaunched.Should().Be(updateVideoApiInput.YearLaunched);
        videoFromDb.Opened.Should().Be(updateVideoApiInput.Opened);
        videoFromDb.Published.Should().Be(updateVideoApiInput.Published);
        videoFromDb.Duration.Should().Be(updateVideoApiInput.Duration);
        videoFromDb.Rating.Should().Be(updateVideoApiInput.Rating!.ToRating());

        var videoCategoriesFromDb = await _fixture.VideoPersistence.GetVideosCategories(targetVideoId);

        var categoriesIdsFromDb = videoCategoriesFromDb!.Select(x => x.CategoryId).ToList();
        
        updateVideoApiInput.CategoriesIds.Should().BeEquivalentTo(categoriesIdsFromDb);

        var videoGenresFromDb = await _fixture.VideoPersistence.GetVideosGenres(targetVideoId);
        var genresIdsFromDb = videoGenresFromDb!.Select(x => x.GenreId).ToList();
        updateVideoApiInput.GenresIds.Should().BeEquivalentTo(genresIdsFromDb);

        var videoCastMembersFromDb = await _fixture.VideoPersistence.GetVideosCastMembers(targetVideoId);
        var castMembersIdsFromDb = videoCastMembersFromDb!.Select(x => x.CastMemberId).ToList();
        updateVideoApiInput.CastMembersIds.Should().BeEquivalentTo(castMembersIdsFromDb);

    }

    [Fact(DisplayName = nameof(Error404WhenVideoIdNotFound))]
    [Trait("EndToEnd/API", "Video/UpdateVideo - Endpoints")]
    public async Task Error404WhenVideoIdNotFound()
    {
        var randomId = Guid.NewGuid();

        var videoCollection = _fixture.GetVideoCollection(10);
        await _fixture.VideoPersistence.InsertList(videoCollection);

        var input = new UpdateVideoApiInput()
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            YearLaunched = _fixture.GetValidYearLaunched(),
            Duration = _fixture.GetValidDuration(),
            Opened = _fixture.GetRandomBoolean(),
            Published = _fixture.GetRandomBoolean(),
            Rating = _fixture.GetRandomRating().ToStringSignal()
        };


        var (response, output) = await _fixture.ApiClient
                     .Put<ProblemDetails>($"/videos/{randomId}", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);

        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found");
        output.Detail.Should().Be($"Video '{randomId}' not found.");
    }

    [Fact(DisplayName = nameof(Error422WhenCategoryIdNotFound))]
    [Trait("EndToEnd/API", "Video/UpdateVideo - Endpoints")]
    public async Task Error422WhenCategoryIdNotFound()
    {
        var examplesVideos = _fixture.GetVideoCollection(10);

        await _fixture.VideoPersistence.InsertList(examplesVideos);

        var videoId = examplesVideos.ElementAt(4);

        var categoryId = Guid.NewGuid();


        var input = new UpdateVideoApiInput()
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            YearLaunched = _fixture.GetValidYearLaunched(),
            Duration = _fixture.GetValidDuration(),
            Opened = _fixture.GetRandomBoolean(),
            Published = _fixture.GetRandomBoolean(),
            Rating = _fixture.GetRandomRating().ToStringSignal(),
            CategoriesIds = new List<Guid>() { categoryId }
        };


        var (response, output) = await _fixture.ApiClient
                     .Put<ProblemDetails>($"/videos/{videoId.Id}", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        output.Should().NotBeNull();
        output!.Type.Should().Be("RelatedAggregate");
        output.Detail.Should().Be($"Related category id (or ids) not found: {string.Join(',', categoryId)}.");
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}
