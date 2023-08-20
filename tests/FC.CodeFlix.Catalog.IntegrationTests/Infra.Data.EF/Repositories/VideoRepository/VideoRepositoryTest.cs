using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Repository = FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.VideoRepository;

[Collection(nameof(VideoRepositoryTestFixture))]
public class VideoRepositoryTest
{
    private readonly VideoRepositoryTestFixture _fixture;

    public VideoRepositoryTest(VideoRepositoryTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetExampleVideo();

        var videoRepository = new Repository.VideoRepository(dbContext);

        await videoRepository.Insert(exampleVideo, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(exampleVideo.Id);

        dbVideo.Should().NotBeNull();
        dbVideo!.Title.Should().Be(exampleVideo.Title);
        dbVideo.Description.Should().Be(exampleVideo.Description);
        dbVideo.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        dbVideo.Opened.Should().Be(exampleVideo.Opened);
        dbVideo.Published.Should().Be(exampleVideo.Published);
        dbVideo.Duration.Should().Be(exampleVideo.Duration);
        dbVideo.Rating.Should().Be(exampleVideo.Rating);
        dbVideo.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(1));

        dbVideo.Thumb.Should().BeNull();
        dbVideo.ThumbHalf.Should().BeNull();
        dbVideo.Banner.Should().BeNull();
        dbVideo.Media.Should().BeNull();
        dbVideo.Trailer.Should().BeNull();

        dbVideo.Genres.Should().BeEmpty();
        dbVideo.Categories.Should().BeEmpty();
        dbVideo.CastMembers.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(InsertWithRelations))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task InsertWithRelations()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetExampleVideo();

        var castMembers = _fixture.GetRandomCastMemberList();
        var categories = _fixture.GetRandomCategoryList();
        var genres = _fixture.GetRandomGenreList();

        castMembers.ToList().ForEach(castMember => exampleVideo.AddCastMember(castMember.Id));
        categories.ToList().ForEach(category => exampleVideo.AddCategory(category.Id));
        genres.ToList().ForEach(genre => exampleVideo.AddGenre(genre.Id));


        await dbContext.CastMembers.AddRangeAsync(castMembers);
        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.Genres.AddRangeAsync(genres);

        await dbContext.SaveChangesAsync();

        var videoRepository = new Repository.VideoRepository(dbContext);

        await videoRepository.Insert(exampleVideo, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(exampleVideo.Id);

        dbVideo.Should().NotBeNull();

        var dbVideosCategories = await assertsDbContext
                                    .VideosCategories
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.CategoryId)
                                    .ToListAsync();

        var dbVideosGenres = await assertsDbContext
                                    .VideosGenres
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.GenreId)
                                    .ToListAsync();

        var dbVideosCastMembers = await assertsDbContext
                                    .VideosCastMembers
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.CastMemberId)
                                    .ToListAsync();

        dbVideosCategories.Should().HaveCount(categories.Count);
        dbVideosCategories.Should().BeEquivalentTo(categories.Select(category => category.Id).ToList());

        dbVideosGenres.Should().HaveCount(genres.Count);
        dbVideosGenres.Should().BeEquivalentTo(genres.Select(genre => genre.Id).ToList());

        dbVideosCastMembers.Should().HaveCount(castMembers.Count);
        dbVideosCastMembers.Should().BeEquivalentTo(castMembers.Select(castMember => castMember.Id).ToList());

    }


    [Fact(DisplayName = nameof(InsertWithMediaAndImages))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task InsertWithMediaAndImages()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        var videoRepository = new Repository.VideoRepository(dbContext);

        await videoRepository.Insert(exampleVideo, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos
            .Include(video => video.Media)
            .Include(video => video.Trailer)
            .FirstOrDefaultAsync(video => video.Id == exampleVideo.Id);



        dbVideo.Should().NotBeNull();

        dbVideo!.Thumb.Should().NotBeNull();
        dbVideo.Thumb!.Path.Should().Be(exampleVideo.Thumb!.Path);

        dbVideo.ThumbHalf.Should().NotBeNull();
        dbVideo.ThumbHalf!.Path.Should().Be(exampleVideo.ThumbHalf!.Path);

        dbVideo.Banner.Should().NotBeNull();
        dbVideo.Banner!.Path.Should().Be(exampleVideo.Banner!.Path);

        dbVideo.Media.Should().NotBeNull();
        dbVideo.Media!.FilePath.Should().Be(exampleVideo.Media!.FilePath);
        dbVideo.Media.EncodedPath.Should().Be(exampleVideo.Media.EncodedPath);
        dbVideo.Media.Status.Should().Be(exampleVideo.Media.Status);

        dbVideo.Trailer.Should().NotBeNull();
        dbVideo.Trailer!.FilePath.Should().Be(exampleVideo.Trailer!.FilePath);
        dbVideo.Trailer.EncodedPath.Should().Be(exampleVideo.Trailer.EncodedPath);
        dbVideo.Trailer.Status.Should().Be(exampleVideo.Trailer.Status);


    }


    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task Update()
    {
        var arrangeDbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetExampleVideo();

        await arrangeDbContext.Videos.AddAsync(exampleVideo);
        await arrangeDbContext.SaveChangesAsync();

        var dbContextAct = _fixture.CreateDbContext(true);
        var videoRepository = new Repository.VideoRepository(dbContextAct);

        var newValuesVideo = _fixture.GetExampleVideo();

        exampleVideo.Update(newValuesVideo.Title,
                            newValuesVideo.Description,
                            newValuesVideo.YearLaunched,
                            newValuesVideo.Opened,
                            newValuesVideo.Published,
                            newValuesVideo.Duration,
                            newValuesVideo.Rating);


        await videoRepository.Update(exampleVideo, CancellationToken.None);
        await dbContextAct.SaveChangesAsync();

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(exampleVideo.Id);

        dbVideo.Should().NotBeNull();
        dbVideo!.Title.Should().Be(exampleVideo.Title);
        dbVideo.Description.Should().Be(exampleVideo.Description);
        dbVideo.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        dbVideo.Opened.Should().Be(exampleVideo.Opened);
        dbVideo.Published.Should().Be(exampleVideo.Published);
        dbVideo.Duration.Should().Be(exampleVideo.Duration);
        dbVideo.Rating.Should().Be(exampleVideo.Rating);

        dbVideo.Thumb.Should().BeNull();
        dbVideo.ThumbHalf.Should().BeNull();
        dbVideo.Banner.Should().BeNull();

        dbVideo.Media.Should().BeNull();
        dbVideo.Trailer.Should().BeNull();

        dbVideo.Genres.Should().BeEmpty();
        dbVideo.Categories.Should().BeEmpty();
        dbVideo.CastMembers.Should().BeEmpty();
    }


    [Fact(DisplayName = nameof(UpdateEntitiesAndValueObjects))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task UpdateEntitiesAndValueObjects()
    {
        var arrangeDbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetExampleVideo();

        exampleVideo.UpdateTrailer(_fixture.GetValidMediaPath());

        await arrangeDbContext.Videos.AddAsync(exampleVideo);
        await arrangeDbContext.SaveChangesAsync();


        var updatedBanner = _fixture.GetValidImagePath();
        var updatedThumb = _fixture.GetValidImagePath();
        var updatedThumbHalf = _fixture.GetValidImagePath();
        var updatedMedia = _fixture.GetValidMediaPath();
        var updatedTrailer = _fixture.GetValidMediaPath();
        var updatedMediaEnconcoded = _fixture.GetValidMediaPath();

        var videoRepository = new Repository.VideoRepository(arrangeDbContext);

        var savedVideo = await arrangeDbContext.Videos.SingleAsync();

        savedVideo!.UpdateBanner(updatedBanner);
        savedVideo.UpdateThumb(updatedThumb);
        savedVideo.UpdateThumbHalf(updatedThumbHalf);
        savedVideo.UpdateMedia(updatedMedia);
        savedVideo.UpdateTrailer(updatedTrailer);
        savedVideo.UpdateAsEncoded(updatedMediaEnconcoded);


        await videoRepository.Update(savedVideo, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync();

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(exampleVideo.Id);

        dbVideo.Should().NotBeNull();


        dbVideo!.Thumb!.Path.Should().Be(updatedThumb);
        dbVideo.ThumbHalf!.Path.Should().Be(updatedThumbHalf);
        dbVideo.Banner!.Path.Should().Be(updatedBanner);

        dbVideo.Media.Should().NotBeNull();
        dbVideo.Media!.FilePath.Should().Be(updatedMedia);
        dbVideo.Media.EncodedPath.Should().Be(updatedMediaEnconcoded);

        dbVideo.Trailer!.FilePath.Should().Be(updatedTrailer);

    }

    [Fact(DisplayName = nameof(UpdateWithRelations))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task UpdateWithRelations()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleVideo = _fixture.GetExampleVideo();

        var castMembers = _fixture.GetRandomCastMemberList();
        var categories = _fixture.GetRandomCategoryList();
        var genres = _fixture.GetRandomGenreList();

        await dbContext.Videos.AddAsync(exampleVideo);

        await dbContext.CastMembers.AddRangeAsync(castMembers);
        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.Genres.AddRangeAsync(genres);

        await dbContext.SaveChangesAsync();

        var videoRepository = new Repository.VideoRepository(dbContext);

        var savedVideo = await dbContext.Videos.FirstAsync(x => x.Id == exampleVideo.Id);

        castMembers.ToList().ForEach(castMember => savedVideo.AddCastMember(castMember.Id));
        categories.ToList().ForEach(category => savedVideo.AddCategory(category.Id));
        genres.ToList().ForEach(genre => savedVideo.AddGenre(genre.Id));

        await videoRepository.Update(savedVideo, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(exampleVideo.Id);

        dbVideo.Should().NotBeNull();

        var dbVideosCategories = await assertsDbContext
                                    .VideosCategories
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.CategoryId)
                                    .ToListAsync();

        var dbVideosGenres = await assertsDbContext
                                    .VideosGenres
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.GenreId)
                                    .ToListAsync();

        var dbVideosCastMembers = await assertsDbContext
                                    .VideosCastMembers
                                    .Where(relation => relation.VideoId == exampleVideo.Id)
                                    .Select(exampleVideo => exampleVideo.CastMemberId)
                                    .ToListAsync();

        dbVideosCategories.Should().HaveCount(categories.Count);
        dbVideosCategories.Should().BeEquivalentTo(categories.Select(category => category.Id).ToList());

        dbVideosGenres.Should().HaveCount(genres.Count);
        dbVideosGenres.Should().BeEquivalentTo(genres.Select(genre => genre.Id).ToList());

        dbVideosCastMembers.Should().HaveCount(castMembers.Count);
        dbVideosCastMembers.Should().BeEquivalentTo(castMembers.Select(castMember => castMember.Id).ToList());

    }


    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task Delete()
    {
        var id = Guid.Empty;
        using (var dbContext = _fixture.CreateDbContext())
        {
            var exampleVideo = _fixture.GetExampleVideo();

            id = exampleVideo.Id;

            await dbContext.Videos.AddAsync(exampleVideo);

            await dbContext.SaveChangesAsync();

        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var savedVideo = await actDbContext.Videos.FirstAsync(x => x.Id == id);

        await videoRepository.Delete(savedVideo, CancellationToken.None);
        await actDbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbVideo = await assertsDbContext.Videos.FindAsync(id);

        dbVideo.Should().BeNull();

    }


    [Fact(DisplayName = nameof(DeleteWithAllPropertiesAndRelations))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task DeleteWithAllPropertiesAndRelations()
    {
        var id = Guid.Empty;
        using (var dbContext = _fixture.CreateDbContext())
        {
            var exampleVideo = _fixture.GetValidVideoWithAllProperties();

            id = exampleVideo.Id;

            var castMembers = _fixture.GetRandomCastMemberList();
            var categories = _fixture.GetRandomCategoryList();
            var genres = _fixture.GetRandomGenreList();

            await dbContext.CastMembers.AddRangeAsync(castMembers);
            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.Genres.AddRangeAsync(genres);

            await dbContext.Videos.AddAsync(exampleVideo);

            castMembers.ToList().ForEach(async castMember =>
                await dbContext.VideosCastMembers.AddAsync(
                    new VideosCastMembers(castMember.Id, id)
            ));

            categories.ToList().ForEach(async category =>
                await dbContext.VideosCategories.AddAsync(
                    new VideosCategories(category.Id, id)
            ));

            genres.ToList().ForEach(async genre =>
                await dbContext.VideosGenres.AddAsync(
                    new VideosGenres(genre.Id, id)
            ));

            await dbContext.SaveChangesAsync();

        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var savedVideo = await actDbContext.Videos.FirstAsync(x => x.Id == id);

        await videoRepository.Delete(savedVideo, CancellationToken.None);
        await actDbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);

        (await assertsDbContext.Videos
            .CountAsync(video => video.Id == id))
            .Should()
            .Be(0);

        (await assertsDbContext.VideosCategories
                                   .CountAsync(relation => relation.VideoId == id))
                                   .Should()
                                   .Be(0);

        (await assertsDbContext.VideosGenres
                                    .CountAsync(relation => relation.VideoId == id))
                                    .Should()
                                    .Be(0);

        (await assertsDbContext
                                    .VideosCastMembers
                                    .CountAsync(relation => relation.VideoId == id))
                                    .Should()
                                    .Be(0);

        (await assertsDbContext.Set<Media>().CountAsync())
                                   .Should()
                                   .Be(0);
    }


    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task Get()
    {
        var exampleVideo = _fixture.GetExampleVideo();
        using (var dbContext = _fixture.CreateDbContext())
        {
            await dbContext.Videos.AddAsync(exampleVideo);
            await dbContext.SaveChangesAsync();

        }

        var videoRepository = new Repository.VideoRepository(
                _fixture.CreateDbContext(true)
            );

        var video = await videoRepository.Get(exampleVideo.Id, CancellationToken.None);

        video.Should().NotBeNull();

        video!.Title.Should().Be(exampleVideo.Title);
        video.Description.Should().Be(exampleVideo.Description);
        video.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        video.Opened.Should().Be(exampleVideo.Opened);
        video.Published.Should().Be(exampleVideo.Published);
        video.Duration.Should().Be(exampleVideo.Duration);
        video.Rating.Should().Be(exampleVideo.Rating);
        video.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(1));

        video.Genres.Should().BeEmpty();
        video.Categories.Should().BeEmpty();
        video.CastMembers.Should().BeEmpty();

        video.ThumbHalf.Should().BeNull();
        video!.Thumb.Should().BeNull();
        video.Banner.Should().BeNull();
        video.Media.Should().BeNull();
        video.Trailer.Should().BeNull();

    }


    [Fact(DisplayName = nameof(GetWithAllProperties))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task GetWithAllProperties()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        exampleVideo.RemoveAllCastMembers();
        exampleVideo.RemoveAllCategories();
        exampleVideo.RemoveAllGenre();

        using (var dbContext = _fixture.CreateDbContext())
        {

            var castMembers = _fixture.GetRandomCastMemberList();
            var categories = _fixture.GetRandomCategoryList();
            var genres = _fixture.GetRandomGenreList();

            await dbContext.CastMembers.AddRangeAsync(castMembers);
            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.Genres.AddRangeAsync(genres);

            await dbContext.Videos.AddAsync(exampleVideo);

            castMembers.ToList().ForEach(async castMember =>
            {
                exampleVideo.AddCastMember(castMember.Id);
                await dbContext.VideosCastMembers.AddAsync(
                    new VideosCastMembers(castMember.Id, exampleVideo.Id));
            });

            categories.ToList().ForEach(async category =>
            {
                exampleVideo.AddCategory(category.Id);
                await dbContext.VideosCategories.AddAsync(
                    new VideosCategories(category.Id, exampleVideo.Id));
            });

            genres.ToList().ForEach(async genre =>
            {
                exampleVideo.AddGenre(genre.Id);
                await dbContext.VideosGenres.AddAsync(
                    new VideosGenres(genre.Id, exampleVideo.Id));
            }
            );

            await dbContext.SaveChangesAsync();
        }

        var videoRepository = new Repository.VideoRepository(
                _fixture.CreateDbContext(true)
            );

        var video = await videoRepository.Get(exampleVideo.Id, CancellationToken.None);

        video.Should().NotBeNull();

        video!.Title.Should().Be(exampleVideo.Title);
        video.Description.Should().Be(exampleVideo.Description);
        video.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        video.Opened.Should().Be(exampleVideo.Opened);
        video.Published.Should().Be(exampleVideo.Published);
        video.Duration.Should().Be(exampleVideo.Duration);
        video.Rating.Should().Be(exampleVideo.Rating);
        video.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(1));

        video.ThumbHalf.Should().NotBeNull();
        video.ThumbHalf!.Path.Should().Be(exampleVideo.ThumbHalf!.Path);

        video!.Thumb.Should().NotBeNull();
        video.Thumb!.Path.Should().Be(exampleVideo.Thumb!.Path);

        video.Banner.Should().NotBeNull();
        video.Banner!.Path.Should().Be(exampleVideo.Banner!.Path);

        video.Media.Should().NotBeNull();
        video.Media!.FilePath.Should().Be(exampleVideo.Media!.FilePath);
        video.Media.EncodedPath.Should().Be(exampleVideo.Media!.EncodedPath);
        video.Media.Status.Should().Be(exampleVideo.Media!.Status);

        video.Trailer.Should().NotBeNull();
        video.Trailer!.FilePath.Should().Be(exampleVideo.Trailer!.FilePath);
        video.Trailer.EncodedPath.Should().Be(exampleVideo.Trailer!.EncodedPath);
        video.Trailer.Status.Should().Be(exampleVideo.Trailer!.Status);

        video.Genres.Should().NotBeNull();
        video.Genres.Should().BeEquivalentTo(exampleVideo.Genres);

        video.Categories.Should().NotBeNull();
        video.Categories.Should().BeEquivalentTo(exampleVideo.Categories);

        video.CastMembers.Should().NotBeNull();
        video.CastMembers.Should().BeEquivalentTo(exampleVideo.CastMembers);
    }


    [Fact(DisplayName = nameof(GetThrowIfNotFound))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task GetThrowIfNotFound()
    {
        var randomId = Guid.NewGuid();

        var videoRepository = new Repository.VideoRepository(
                _fixture.CreateDbContext(true)
            );

        var video = () => videoRepository.Get(randomId, CancellationToken.None);


        await video.Should()
                   .ThrowExactlyAsync<NotFoundException>()
                   .WithMessage($"Video '{randomId}' not found.");

    }


    [Fact(DisplayName = nameof(Search))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task Search()
    {
        var exampleVideosList = _fixture.GetExampleVideosList();

        using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            await arrangeDbContext.Videos.AddRangeAsync(exampleVideosList);
            await arrangeDbContext.SaveChangesAsync();
        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var input = new SearchInput(1, 20, "", "", default);

        var result = await videoRepository.Search(input, CancellationToken.None);

        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Total.Should().Be(exampleVideosList.Count);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(exampleVideosList.Count);


        result.Items.ToList().ForEach(resultItem =>
        {
            var exampleVideo = exampleVideosList.FirstOrDefault(id => id.Id == resultItem.Id);
            exampleVideo.Should().NotBeNull();

            resultItem.Should().NotBeNull();
            resultItem!.Title.Should().Be(exampleVideo!.Title);
            resultItem.Description.Should().Be(exampleVideo.Description);
            resultItem.YearLaunched.Should().Be(exampleVideo.YearLaunched);
            resultItem.Opened.Should().Be(exampleVideo.Opened);
            resultItem.Published.Should().Be(exampleVideo.Published);
            resultItem.Duration.Should().Be(exampleVideo.Duration);
            resultItem.Rating.Should().Be(exampleVideo.Rating);
            resultItem.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(1));

            resultItem.Thumb.Should().BeNull();
            resultItem.ThumbHalf.Should().BeNull();
            resultItem.Banner.Should().BeNull();
            resultItem.Media.Should().BeNull();
            resultItem.Trailer.Should().BeNull();

            resultItem.Genres.Should().BeEmpty();
            resultItem.Categories.Should().BeEmpty();
            resultItem.CastMembers.Should().BeEmpty();


        });

    }


    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenEmpty))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenEmpty()
    {
        var actDbContext = _fixture.CreateDbContext(false);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var input = new SearchInput(1, 20, "", "", default);

        var result = await videoRepository.Search(input, CancellationToken.None);

        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Total.Should().Be(0);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(0);

    }


    [Theory(DisplayName = nameof(SearchPagination))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchPagination(int quantityToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)

    {
        var exampleVideosList = _fixture.GetExampleVideosList(quantityToGenerate);

        using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            await arrangeDbContext.Videos.AddRangeAsync(exampleVideosList);
            await arrangeDbContext.SaveChangesAsync();
        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var input = new SearchInput(page, perPage, "", "", default);

        var result = await videoRepository.Search(input, CancellationToken.None);

        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Total.Should().Be(exampleVideosList.Count);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(expectedQuantityItems);


        result.Items
            .ToList()
            .ForEach(resultItem =>
            {
                var exampleVideo = exampleVideosList.FirstOrDefault(id => id.Id == resultItem.Id);
                exampleVideo.Should().NotBeNull();

                resultItem.Should().NotBeNull();
                resultItem!.Title.Should().Be(exampleVideo!.Title);
                resultItem.Description.Should().Be(exampleVideo.Description);
            });
    }

    [Theory(DisplayName = nameof(SearchByTitle))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    [InlineData("action", 1, 5, 1, 1)]
    [InlineData("horror", 1, 5, 3, 3)]
    [InlineData("horror", 2, 5, 0, 3)]
    [InlineData("sci-fi", 1, 5, 4, 4)]
    [InlineData("sci-fi", 1, 2, 2, 4)]
    [InlineData("sci-fi", 2, 3, 1, 4)]
    [InlineData("sci-fi other", 1, 3, 0, 0)]
    [InlineData("robots", 1, 5, 2, 2)]
    [InlineData("", 1, 5, 5, 9)]
    [InlineData("teste-no-items", 1, 5, 0, 0)]
    public async Task SearchByTitle(string search,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItemsReturned,
                                             int expectedQuantityTotalItems)

    {
        var exampleVideosList = _fixture.GetExampleVideosListByTitles(new() {
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

        using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            await arrangeDbContext.Videos.AddRangeAsync(exampleVideosList);
            await arrangeDbContext.SaveChangesAsync();
        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var input = new SearchInput(page, perPage, search, "", default);

        var result = await videoRepository.Search(input, CancellationToken.None);

        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Total.Should().Be(expectedQuantityTotalItems);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(expectedQuantityItemsReturned);


        result.Items
            .ToList()
            .ForEach(resultItem =>
            {
                var exampleVideo = exampleVideosList.FirstOrDefault(id => id.Id == resultItem.Id);
                exampleVideo.Should().NotBeNull();

                resultItem.Should().NotBeNull();
                resultItem!.Title.Should().Be(exampleVideo!.Title);
                resultItem.Description.Should().Be(exampleVideo.Description);
            });
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    [InlineData("title", "asc")]
    [InlineData("title", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var exampleVideosList = _fixture.GetExampleVideosList(10);

        using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            await arrangeDbContext.Videos.AddRangeAsync(exampleVideosList);
            await arrangeDbContext.SaveChangesAsync();
        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var searchOrder = order.ToLower() == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new SearchInput(1, 20, "", orderBy, searchOrder);

        var result = await videoRepository.Search(input, CancellationToken.None);

        var orderedList = VideoRepositoryTestFixture.CloneListOrdered(exampleVideosList, input);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Items.Should().HaveCount(exampleVideosList.Count);
        result.Total.Should().Be(exampleVideosList.Count);

        for (int i = 0; i < orderedList.Count; i++)
        {
            var expectedItem = orderedList[i];

            var outputItem = result.Items[i];

            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();

            outputItem.Title.Should().Be(expectedItem!.Title);
            outputItem.Id.Should().Be(expectedItem!.Id);

            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }


    [Fact(DisplayName = nameof(SearchReturnsAllRelations))]
    [Trait("Integration/Infra.Data", "VideoRepository - Repositories")]
    public async Task SearchReturnsAllRelations()
    {
        var exampleVideosList = _fixture.GetExampleVideosList();

        using (var arrageDbContext = _fixture.CreateDbContext())
        {
            foreach (var exampleVideo in exampleVideosList)
            {
                var castMembers = _fixture.GetRandomCastMemberList();
                var categories = _fixture.GetRandomCategoryList();
                var genres = _fixture.GetRandomGenreList();

                await arrageDbContext.CastMembers.AddRangeAsync(castMembers);
                await arrageDbContext.Categories.AddRangeAsync(categories);
                await arrageDbContext.Genres.AddRangeAsync(genres);

                castMembers.ToList().ForEach(async castMember =>
                {
                    exampleVideo.AddCastMember(castMember.Id);
                    await arrageDbContext.VideosCastMembers.AddAsync(
                        new VideosCastMembers(castMember.Id, exampleVideo.Id));
                });

                categories.ToList().ForEach(async category =>
                {
                    exampleVideo.AddCategory(category.Id);
                    await arrageDbContext.VideosCategories.AddAsync(
                        new VideosCategories(category.Id, exampleVideo.Id));
                });

                genres.ToList().ForEach(async genre =>
                {
                    exampleVideo.AddGenre(genre.Id);
                    await arrageDbContext.VideosGenres.AddAsync(
                        new VideosGenres(genre.Id, exampleVideo.Id));
                }
                );
            }

            await arrageDbContext.Videos.AddRangeAsync(exampleVideosList);
            await arrageDbContext.SaveChangesAsync();
        }

        var actDbContext = _fixture.CreateDbContext(true);

        var videoRepository = new Repository.VideoRepository(actDbContext);

        var input = new SearchInput(1, 20, "", "", default);

        var result = await videoRepository.Search(input, CancellationToken.None);

        result.CurrentPage.Should().Be(input.Page);
        result.PerPage.Should().Be(input.PerPage);
        result.Total.Should().Be(exampleVideosList.Count);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(exampleVideosList.Count);


        result.Items.ToList().ForEach(resultItem =>
        {
            var exampleVideo = exampleVideosList.FirstOrDefault(id => id.Id == resultItem.Id);
            exampleVideo.Should().NotBeNull();

            resultItem.Genres.Should().BeEquivalentTo(exampleVideo!.Genres);
            resultItem.Categories.Should().BeEquivalentTo(exampleVideo!.Categories);
            resultItem.CastMembers.Should().BeEquivalentTo(exampleVideo!.CastMembers);

        });
    }
}