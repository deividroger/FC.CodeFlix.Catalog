using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.CreateVideo;

[Collection(nameof(VideoBaseFixture))]
public class CreateVideoApiTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public CreateVideoApiTest(VideoBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateBasicVideo))]
    [Trait("EndToEnd/API", "Video/Create - Endpoints")]
    public async Task CreateBasicVideo()
    {
        var input = _fixture.GetBasicCreateVideoInput();

        var (response, output) = await _fixture.ApiClient
                      .Post<ApiResponse<VideoModelOutput>>($"/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Data.Id.Should().NotBeEmpty();
        output.Data.Title.Should().Be(input.Title);
        output.Data.Description.Should().Be(input.Description);
        output.Data.YearLaunched.Should().Be(input.YearLaunched);
        output.Data.Opened.Should().Be(input.Opened);
        output.Data.Published.Should().Be(input.Published);
        output.Data.Duration.Should().Be(input.Duration);
        output.Data.Rating.Should().Be(input.Rating);

        var videoFromDb = await _fixture.VideoPersistence.GetById(output.Data.Id);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Id.Should().NotBeEmpty();
        videoFromDb.Title.Should().Be(input.Title);
        videoFromDb.Description.Should().Be(input.Description);
        videoFromDb.YearLaunched.Should().Be(input.YearLaunched);
        videoFromDb.Opened.Should().Be(input.Opened);
        videoFromDb.Published.Should().Be(input.Published);
        videoFromDb.Duration.Should().Be(input.Duration);
        videoFromDb.Rating.Should().Be(input.Rating!.ToRating());


    }

    [Fact(DisplayName = nameof(CreateBasicVideoWithRelationships))]
    [Trait("EndToEnd/API", "Video/Create - Endpoints")]
    public async Task CreateBasicVideoWithRelationships()
    {
        var categories = _fixture.GetExampleCategoriesList();

        await _fixture.CategoryPersistence.InsertList(categories);

        var genres = _fixture.GetExampleListGenres();

        await _fixture.GenrePersistence.InsertList(genres);

        var castMembers = _fixture.GetExampleCastMemberList();

        await _fixture.CastMemberPersistence.InsertList(castMembers);


        var input = _fixture.GetBasicCreateVideoInput();

        input.CategoriesIds = categories.Select(category => category.Id).ToList();
        input.GenresIds = genres.Select(genre => genre.Id).ToList();
        input.CastMembersIds = castMembers.Select(castMember => castMember.Id).ToList();

        var (response, output) = await _fixture.ApiClient
                      .Post<ApiResponse<VideoModelOutput>>("/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Data.Id.Should().NotBeEmpty();
        output.Data.Title.Should().Be(input.Title);
        output.Data.Description.Should().Be(input.Description);
        output.Data.YearLaunched.Should().Be(input.YearLaunched);
        output.Data.Opened.Should().Be(input.Opened);
        output.Data.Published.Should().Be(input.Published);
        output.Data.Duration.Should().Be(input.Duration);
        output.Data.Rating.Should().Be(input.Rating);

        var outputCategoriesIds = output.Data!.Categories!.Select(category => category.Id).ToList();
        outputCategoriesIds.Should().NotBeNull();
        outputCategoriesIds.Should().BeEquivalentTo(input.CategoriesIds);

        var outputGenresIds = output.Data!.Genres!.Select(genre => genre.Id).ToList();
        outputGenresIds.Should().NotBeNull();
        outputGenresIds.Should().BeEquivalentTo(input.GenresIds);


        var outputCastMembersIds = output.Data!.CastMembers!.Select(castMember => castMember.Id).ToList();
        outputCastMembersIds.Should().NotBeNull();
        outputCastMembersIds.Should().BeEquivalentTo(input.CastMembersIds);


        var videoFromDb = await _fixture.VideoPersistence.GetById(output.Data.Id);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Id.Should().NotBeEmpty();
        videoFromDb.Title.Should().Be(input.Title);
        videoFromDb.Description.Should().Be(input.Description);
        videoFromDb.YearLaunched.Should().Be(input.YearLaunched);
        videoFromDb.Opened.Should().Be(input.Opened);
        videoFromDb.Published.Should().Be(input.Published);
        videoFromDb.Duration.Should().Be(input.Duration);
        videoFromDb.Rating.Should().Be(input.Rating!.ToRating());

        var videoCategoriesFromDb = await _fixture.VideoPersistence.
            GetVideosCategories(videoFromDb.Id);

        videoCategoriesFromDb.Should().NotBeNull();

        var categoriesIdsFromDb = videoCategoriesFromDb!.Select(x => x.CategoryId).ToList();
        categoriesIdsFromDb.Should().BeEquivalentTo(input.CategoriesIds);

        var videoGenresFromDb = await _fixture.VideoPersistence.
            GetVideosGenres(videoFromDb.Id);

        videoGenresFromDb.Should().NotBeNull();
        var genresIdsFromDb = videoGenresFromDb!.Select(x => x.GenreId).ToList();

        genresIdsFromDb.Should().BeEquivalentTo(input.GenresIds);

        var videoCastMembersFromDb = await _fixture.VideoPersistence.
            GetVideosCastMembers(videoFromDb.Id);

        videoCastMembersFromDb.Should().NotBeNull();
        var castMembersIdsFromDb = videoCastMembersFromDb!.Select(x => x.CastMemberId).ToList();

        castMembersIdsFromDb.Should().BeEquivalentTo(input.CastMembersIds);

    }


    [Fact(DisplayName = nameof(CreateVideoWithInvalidGenreId))]
    [Trait("EndToEnd/API", "Video/Create - Endpoints")]
    public async Task CreateVideoWithInvalidGenreId()
    {
        var invalidGenreId = Guid.NewGuid();
        var input = _fixture.GetBasicCreateVideoInput();

        input.GenresIds = new List<Guid>() { invalidGenreId };

        var (response, output) = await _fixture.ApiClient
                      .Post<ProblemDetails>("/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        output.Should().NotBeNull();
        output.Type.Should().Be("RelatedAggregate");
        output!.Detail.Should().Be($"Related genre id (or ids) not found: {string.Join(',', invalidGenreId)}.");
    }

    [Fact(DisplayName = nameof(CreateVideoWithInvalidCategoryId))]
    [Trait("EndToEnd/API", "Video/Create - Endpoints")]
    public async Task CreateVideoWithInvalidCategoryId()
    {
        var invalidCategoryId = Guid.NewGuid();
        var input = _fixture.GetBasicCreateVideoInput();

        input.CategoriesIds = new List<Guid>() { invalidCategoryId };

        var (response, output) = await _fixture.ApiClient
                      .Post<ProblemDetails>("/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        output.Should().NotBeNull();
        output.Type.Should().Be("RelatedAggregate");
        output!.Detail.Should().Be($"Related category id (or ids) not found: {string.Join(',', invalidCategoryId)}.");
    }


    [Fact(DisplayName = nameof(CreateVideoWithInvalidCastMemberId))]
    [Trait("EndToEnd/API", "Video/Create - Endpoints")]
    public async Task CreateVideoWithInvalidCastMemberId()
    {
        var invalidCastMemberId = Guid.NewGuid();
        var input = _fixture.GetBasicCreateVideoInput();

        input.CastMembersIds = new List<Guid>() { invalidCastMemberId };

        var (response, output) = await _fixture.ApiClient
                      .Post<ProblemDetails>("/videos", input);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        output.Should().NotBeNull();
        output!.Type.Should().Be("RelatedAggregate");
        output!.Detail.Should().Be($"Related castMember id (or ids) not found: {string.Join(',', invalidCastMemberId)}.");
    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }
}
