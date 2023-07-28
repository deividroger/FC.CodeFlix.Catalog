using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Lw.Collections.Generic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.ListVideos;

[Collection(nameof(ListVideosTestFixture))]
public class ListVideosTest
{
    private readonly ListVideosTestFixture _fixture;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IGenreRepository> _genreRepositoryMock;
    private readonly Mock<ICastMemberRepository> _castMemberRepositoryMock;
    private readonly UseCase.ListVideos _useCase;

    public ListVideosTest(ListVideosTestFixture fixture)
    {
        _fixture = fixture;

        _videoRepositoryMock = new();
        _categoryRepositoryMock = new();
        _genreRepositoryMock = new();
        _castMemberRepositoryMock = new();
        _useCase = new(_videoRepositoryMock.Object,
                        _categoryRepositoryMock.Object,
                        _genreRepositoryMock.Object,
                        _castMemberRepositoryMock.Object);
    }

    [Fact(DisplayName = nameof(ListVideos))]
    [Trait("Application", "ListVideos - UseCases")]
    public async Task ListVideos()
    {
        var exampleVideosList = _fixture.CreateExamplesVideosList();
        var input = new UseCase.ListVideosInput(1, 10, "", "", SearchOrder.ASC);

        _videoRepositoryMock.Setup(x =>
            x.Search(It.Is<SearchInput>(x =>
             x.Page == input.Page &&
             x.PerPage == input.PerPage &&
             x.Search == input.Search &&
             x.OrderBy == input.Sort &&
             x.Order == input.Dir), It.IsAny<CancellationToken>())).ReturnsAsync(
            new SearchOutput<Catalog.Domain.Entity.Video>(input.Page,
                                                          input.PerPage,
                                                          exampleVideosList.Count,
                                                          exampleVideosList));


        var output = await _useCase.Handle(input, CancellationToken.None);


        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleVideosList.Count);

        output.Items.Should().HaveCount(exampleVideosList.Count);

        output.Items.ForEach((item) =>
        {
            var exampleItem = exampleVideosList.Find(x => x.Id == item.Id);
            exampleItem.Should().NotBeNull();

            item.Id.Should().Be(exampleItem!.Id);
            item.Title.Should().Be(exampleItem.Title);
            item.Description.Should().Be(exampleItem.Description);
            item.YearLaunched.Should().Be(exampleItem.YearLaunched);
            item.Opened.Should().Be(exampleItem.Opened);
            item.Rating.Should().Be(exampleItem.Rating.ToStringSignal());
            item.Duration.Should().Be(exampleItem.Duration);

            item.CreatedAt.Should().Be(exampleItem.CreatedAt);

            item.ThumbFileUrl.Should().Be(exampleItem.Thumb!.Path);
            item.BannerFileUrl.Should().Be(exampleItem.Banner!.Path);
            item.ThumbHalfFileUrl.Should().Be(exampleItem.ThumbHalf!.Path);
            item.VideoFileUrl.Should().Be(exampleItem.Media!.FilePath);
            item.TrailerFileUrl.Should().Be(exampleItem.Trailer!.FilePath);

            item.Categories!
                      .Select(dto => dto.Id)
                       .ToList().Should().BeEquivalentTo(exampleItem.Categories);

            item.Genres!
                      .Select(dto => dto.Id)
                       .ToList().Should().BeEquivalentTo(exampleItem.Genres);

            item.CastMembers!
                      .Select(dto => dto.Id)
                       .ToList().Should().BeEquivalentTo(exampleItem.CastMembers);

        });

        _videoRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(ListReturnsEmptyWhenThereIsNoVideos))]
    [Trait("Application", "ListVideos - UseCases")]
    public async Task ListReturnsEmptyWhenThereIsNoVideos()
    {
        var exampleVideosList = new List<Catalog.Domain.Entity.Video>();

        var input = new UseCase.ListVideosInput(1, 10, "", "", SearchOrder.ASC);

        _videoRepositoryMock.Setup(x =>
            x.Search(It.Is<SearchInput>(x =>
             x.Page == input.Page &&
             x.PerPage == input.PerPage &&
             x.Search == input.Search &&
             x.OrderBy == input.Sort &&
             x.Order == input.Dir), It.IsAny<CancellationToken>())).ReturnsAsync(
            new SearchOutput<Catalog.Domain.Entity.Video>(input.Page,
                                                          input.PerPage,
                                                          exampleVideosList.Count,
                                                          exampleVideosList));


        var output = await _useCase.Handle(input, CancellationToken.None);


        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleVideosList.Count);

        output.Items.Should().HaveCount(exampleVideosList.Count);

        _videoRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(ListVideosWithRelations))]
    [Trait("Application", "ListVideos - UseCases")]
    public async Task ListVideosWithRelations()
    {
        var (exampleVideosList, examplesCategories, examplesGenres, examplesCastMember) =
            _fixture.CreateExamplesVideosListWithRelations();

        var examplesCategoriesIds = examplesCategories
            .Select(category => category.Id)
            .ToList();

        var examplesGenresIds = examplesGenres
            .Select(genre => genre.Id)
            .ToList();

        var examplesCastMembersIds = examplesCastMember
            .Select(castMember => castMember.Id)
            .ToList();

        var input = new UseCase.ListVideosInput(1, 10, "", "", SearchOrder.ASC);

        _categoryRepositoryMock.Setup(x => x.GetListByIds(It.Is<List<Guid>>(
            list => list.All(examplesCategoriesIds.Contains) &&
                             list.Count == examplesCategories.Count
                             ),
                                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(examplesCategories);


        _genreRepositoryMock.Setup(x => x.GetListByIds(It.Is<List<Guid>>(
            list => list.All(examplesGenresIds.Contains) &&
                             list.Count == examplesGenres.Count
                             ),
                             It.IsAny<CancellationToken>()))
            .ReturnsAsync(examplesGenres);

        _castMemberRepositoryMock.Setup(x => x.GetListByIds(It.Is<List<Guid>>(
            list => list.All(examplesCastMembersIds.Contains) &&
                             list.Count == examplesCastMember.Count
                             ),
                             It.IsAny<CancellationToken>()))
            .ReturnsAsync(examplesCastMember);

        _videoRepositoryMock.Setup(x =>
            x.Search(It.Is<SearchInput>(x =>
             x.Page == input.Page &&
             x.PerPage == input.PerPage &&
             x.Search == input.Search &&
             x.OrderBy == input.Sort &&
             x.Order == input.Dir), It.IsAny<CancellationToken>())).ReturnsAsync(
            new SearchOutput<Catalog.Domain.Entity.Video>(input.Page,
                                                          input.PerPage,
                                                          exampleVideosList.Count,
                                                          exampleVideosList));


        var output = await _useCase.Handle(input, CancellationToken.None);


        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleVideosList.Count);

        output.Items.Should().HaveCount(exampleVideosList.Count);

        output.Items.ForEach((outputItem) =>
        {
            var exampleItem = exampleVideosList.Find(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Title.Should().Be(exampleItem.Title);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.YearLaunched.Should().Be(exampleItem.YearLaunched);
            outputItem.Opened.Should().Be(exampleItem.Opened);
            outputItem.Rating.Should().Be(exampleItem.Rating.ToStringSignal());
            outputItem.Duration.Should().Be(exampleItem.Duration);

            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);

            outputItem.ThumbFileUrl.Should().Be(exampleItem.Thumb!.Path);
            outputItem.BannerFileUrl.Should().Be(exampleItem.Banner!.Path);
            outputItem.ThumbHalfFileUrl.Should().Be(exampleItem.ThumbHalf!.Path);
            outputItem.VideoFileUrl.Should().Be(exampleItem.Media!.FilePath);
            outputItem.TrailerFileUrl.Should().Be(exampleItem.Trailer!.FilePath);


            outputItem.Categories.ForEach(relation =>
            {
                var exampleCategory = examplesCategories.Find(x => x.Id == relation.Id);
                exampleCategory.Should().NotBeNull();
                relation.Name.Should().Be(exampleCategory?.Name);

            });

            outputItem.Genres.ForEach(relation =>
            {
                var exampleGenre = examplesGenres.Find(x => x.Id == relation.Id);
                exampleGenre.Should().NotBeNull();
                relation.Name.Should().Be(exampleGenre?.Name);

            });

            outputItem.CastMembers.ForEach(relation =>
            {
                var exampleCastMember = examplesCastMember.Find(x => x.Id == relation.Id);
                exampleCastMember.Should().NotBeNull();
                relation.Name.Should().Be(exampleCastMember?.Name);

            });

        });

        _videoRepositoryMock.VerifyAll();
        _categoryRepositoryMock.VerifyAll();
        _genreRepositoryMock.VerifyAll();
        _castMemberRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(ListVideosWithoutRelationsDoentCallOtherRepositories))]
    [Trait("Application", "ListVideos - UseCases")]
    public async Task ListVideosWithoutRelationsDoentCallOtherRepositories()
    {
        var exampleVideos = _fixture.CreateExampleVideoListWithoutRelations();


        var input = new UseCase.ListVideosInput(1, 10, "", "", SearchOrder.ASC);

        _videoRepositoryMock.Setup(x =>
            x.Search(It.Is<SearchInput>(x =>
             x.Page == input.Page &&
             x.PerPage == input.PerPage &&
             x.Search == input.Search &&
             x.OrderBy == input.Sort &&
             x.Order == input.Dir), It.IsAny<CancellationToken>())).ReturnsAsync(
            new SearchOutput<Catalog.Domain.Entity.Video>(input.Page,
                                                          input.PerPage,
                                                          exampleVideos.Count,
                                                          exampleVideos));


        var output = await _useCase.Handle(input, CancellationToken.None);


        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleVideos.Count);

        output.Items.Should().HaveCount(exampleVideos.Count);

        output.Items.ForEach((outputItem) =>
        {
            var exampleItem = exampleVideos.Find(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Title.Should().Be(exampleItem.Title);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.YearLaunched.Should().Be(exampleItem.YearLaunched);
            outputItem.Opened.Should().Be(exampleItem.Opened);
            outputItem.Rating.Should().Be(exampleItem.Rating.ToStringSignal());
            outputItem.Duration.Should().Be(exampleItem.Duration);

            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);

            outputItem.Categories.Should().HaveCount(0);
            outputItem.Genres.Should().HaveCount(0);
            outputItem.CastMembers.Should().HaveCount(0);
        });

        _videoRepositoryMock.VerifyAll();
        _categoryRepositoryMock.Verify(x 
            => x.GetListByIds(It.IsAny<List<Guid>>(), 
                              It.IsAny<CancellationToken>()), 
            Times.Never);

        _genreRepositoryMock.Verify(x
            => x.GetListByIds(It.IsAny<List<Guid>>(),
                              It.IsAny<CancellationToken>()),
            Times.Never);
        _castMemberRepositoryMock.Verify(x
            => x.GetListByIds(It.IsAny<List<Guid>>(),
                              It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
