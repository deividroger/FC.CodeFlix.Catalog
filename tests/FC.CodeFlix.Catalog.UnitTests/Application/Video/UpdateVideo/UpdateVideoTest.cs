using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntities = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UpdateVideo;

[Collection(nameof(UpdateVideoTestFixture))]
public class UpdateVideoTest
{
    private readonly UpdateVideoTestFixture _fixture;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IGenreRepository> _genreRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICastMemberRepository> _castMemberRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UseCase.UpdateVideo _useCase;

    public UpdateVideoTest(UpdateVideoTestFixture fixture)
    {
        _fixture = fixture;

        _videoRepositoryMock = new();
        _genreRepositoryMock = new();
        _unitOfWorkMock = new();
        _categoryRepositoryMock = new();
        _castMemberRepositoryMock = new();
        _storageServiceMock = new();

        _useCase = new(_videoRepositoryMock.Object,
                        _genreRepositoryMock.Object,
                        _categoryRepositoryMock.Object,
                        _castMemberRepositoryMock.Object,
                        _storageServiceMock.Object,
                       _unitOfWorkMock.Object);
    }

    [Fact(DisplayName = nameof(UpdateVideoBasicInfo))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoBasicInfo()
    {
        var exampleVideo = _fixture.GetValidVideo();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);


    }

    [Theory(DisplayName = nameof(UpdateVideoThrowsWhenReceiveInvalidInput))]
    [Trait("Application", "UpdateVideo - UseCases")]
    [ClassData(typeof(UpdateVideoTestDataGenerator))]
    public async Task UpdateVideoThrowsWhenReceiveInvalidInput(UpdateVideoInput invalidInput, string expectedExceptionMessage)
    {
        var exampleVideo = _fixture.GetValidVideo();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.IsAny<Guid>(),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var action = () => _useCase.Handle(invalidInput, CancellationToken.None);

        var exceptionAssertion = await action.Should().ThrowAsync<EntityValidationException>()
              .WithMessage("There are validation errors");

        exceptionAssertion.Which.Errors!
                .First()
                .Message
                .Should()
                .Be(expectedExceptionMessage);

        _videoRepositoryMock.VerifyAll();
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(UpdateVideoWithGenreIds))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithGenreIds()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var examplesGenresIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);

        _genreRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesGenresIds.Count &&
                                                 idsList.All(id => examplesGenresIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesGenresIds);

        var input = _fixture.CreateValidInput(exampleVideo.Id, examplesGenresIds);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Genres.All(genredId => examplesGenresIds.Contains(genredId)) &&
                                                      Video.Genres.Count == examplesGenresIds.Count
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _genreRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Genres?.Select(x => x.Id).Should().BeEquivalentTo(examplesGenresIds);

    }

    [Fact(DisplayName = nameof(UpdateVideoWithoutRelationsWithRelations))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithoutRelationsWithRelations()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var examplesGenresIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();
        var examplesCastMembersIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();
        var examplesCategoriesIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);

        _genreRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesGenresIds.Count &&
                                                 idsList.All(id => examplesGenresIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesGenresIds);

        _castMemberRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCastMembersIds.Count &&
                                                 idsList.All(id => examplesCastMembersIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCastMembersIds);

        _categoryRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCategoriesIds.Count &&
                                                 idsList.All(id => examplesCategoriesIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCategoriesIds);

        var input = _fixture.CreateValidInput(exampleVideo.Id,
                        genreIds: examplesGenresIds,
                        castMemberIds: examplesCastMembersIds,
                        categoriesIds: examplesCategoriesIds);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Genres.All(genredId => examplesGenresIds.Contains(genredId)) &&
                                                      Video.Genres.Count == examplesGenresIds.Count &&
                                                      Video.CastMembers.All(castMemberId => examplesCastMembersIds.Contains(castMemberId)) &&
                                                      Video.CastMembers.Count == examplesCastMembersIds.Count &&

                                                      Video.Categories.All(categoryId => examplesCategoriesIds.Contains(categoryId)) &&
                                                      Video.Categories.Count == examplesCategoriesIds.Count
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _genreRepositoryMock.VerifyAll();
        _castMemberRepositoryMock.VerifyAll();
        _categoryRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Genres?.Select(x => x.Id).Should().BeEquivalentTo(examplesGenresIds);
        output.CastMembers?.Select(x => x.Id).Should().BeEquivalentTo(examplesCastMembersIds);
        output.Categories?.Select(x => x.Id).Should().BeEquivalentTo(examplesCategoriesIds);
    }

    [Fact(DisplayName = nameof(UpdateVideoWithRelationsRemovingRelations))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithRelationsRemovingRelations()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id,
                        genreIds: new(),
                        castMemberIds: new(),
                        categoriesIds: new());

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Genres.Count == 0 &&
                                                      Video.CastMembers.Count == 0 &&
                                                      Video.Categories.Count == 0
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);


        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

        _genreRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _castMemberRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Genres?.Should().BeEmpty();
        output.CastMembers?.Should().BeEmpty();
        output.Categories?.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(UpdateVideoWithRelationsKeepRelationWhenReceiveNullInRelations))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithRelationsKeepRelationWhenReceiveNullInRelations()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id,
                        genreIds: null,
                        castMemberIds: null,
                        categoriesIds: null);

        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Genres?.Select(x => x.Id).Should().BeEquivalentTo(exampleVideo.Genres);
        output.CastMembers?.Select(x => x.Id).Should().BeEquivalentTo(exampleVideo.CastMembers);
        output.Categories?.Select(x => x.Id).Should().BeEquivalentTo(exampleVideo.Categories);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Genres.Count == exampleVideo.Genres.Count &&
                                                      Video.CastMembers.Count == exampleVideo.CastMembers.Count &&
                                                      Video.Categories.Count == exampleVideo.Categories.Count &&
                                                      Video.Genres.All(genredId => exampleVideo.Genres.Contains(genredId)) &&
                                                      Video.Categories.All(categoryId => exampleVideo.Categories.Contains(categoryId)) &&
                                                      Video.CastMembers.All(castMemberId => exampleVideo.CastMembers.Contains(castMemberId))
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);


        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

        _genreRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _castMemberRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact(DisplayName = nameof(UpdateVideoWithRelationsToOtherRelations))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithRelationsToOtherRelations()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        var examplesGenresIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();
        var examplesCastMembersIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();
        var examplesCategoriesIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);

        _genreRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesGenresIds.Count &&
                                                 idsList.All(id => examplesGenresIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesGenresIds);

        _castMemberRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCastMembersIds.Count &&
                                                 idsList.All(id => examplesCastMembersIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCastMembersIds);

        _categoryRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCategoriesIds.Count &&
                                                 idsList.All(id => examplesCategoriesIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCategoriesIds);

        var input = _fixture.CreateValidInput(exampleVideo.Id,
                        genreIds: examplesGenresIds,
                        castMemberIds: examplesCastMembersIds,
                        categoriesIds: examplesCategoriesIds);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Genres.All(genredId => examplesGenresIds.Contains(genredId)) &&
                                                      Video.Genres.Count == examplesGenresIds.Count &&
                                                      Video.CastMembers.All(castMemberId => examplesCastMembersIds.Contains(castMemberId)) &&
                                                      Video.CastMembers.Count == examplesCastMembersIds.Count &&
                                                      Video.Categories.All(categoryId => examplesCategoriesIds.Contains(categoryId)) &&
                                                      Video.Categories.Count == examplesCategoriesIds.Count
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _genreRepositoryMock.VerifyAll();
        _castMemberRepositoryMock.VerifyAll();
        _categoryRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Genres?.Select(x => x.Id).Should().BeEquivalentTo(examplesGenresIds);
        output.CastMembers?.Select(x => x.Id).Should().BeEquivalentTo(examplesCastMembersIds);
        output.Categories?.Select(x => x.Id).Should().BeEquivalentTo(examplesCategoriesIds);
    }

    [Fact(DisplayName = nameof(UpdateVideoWithCategoriesIds))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithCategoriesIds()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var examplesCategoriesIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);

        _categoryRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCategoriesIds.Count &&
                                                 idsList.All(id => examplesCategoriesIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCategoriesIds);

        var input = _fixture.CreateValidInput(exampleVideo.Id, categoriesIds: examplesCategoriesIds);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Categories.All(genredId => examplesCategoriesIds.Contains(genredId)) &&
                                                      Video.Categories.Count == examplesCategoriesIds.Count
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _categoryRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.Categories?.Select(x => x.Id).Should().BeEquivalentTo(examplesCategoriesIds);

    }

    [Fact(DisplayName = nameof(UpdateVideoWithCastMembersIds))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithCastMembersIds()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var examplesCastMembersIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);

        _castMemberRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.Is<List<Guid>>(idsList => idsList.Count == examplesCastMembersIds.Count &&
                                                 idsList.All(id => examplesCastMembersIds.Contains(id))),
                              It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCastMembersIds);

        var input = _fixture.CreateValidInput(exampleVideo.Id, castMemberIds: examplesCastMembersIds);

        var output = await _useCase.Handle(input, CancellationToken.None);

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.CastMembers.All(castMemberId => examplesCastMembersIds.Contains(castMemberId)) &&
                                                      Video.CastMembers.Count == examplesCastMembersIds.Count
                                                      ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _videoRepositoryMock.VerifyAll();
        _castMemberRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.CastMembers?.Select(x => x.Id).Should().BeEquivalentTo(examplesCastMembersIds);

    }

    [Fact(DisplayName = nameof(UpdateVideosThrowsWhenInvalidCastMemberId))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideosThrowsWhenInvalidCastMemberId()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var invalidCastMemberId = Guid.NewGuid();

        var examplesCastMembersIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        var inputInvalidIdsList = examplesCastMembersIds.Concat(new List<Guid> { invalidCastMemberId }).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, castMemberIds: inputInvalidIdsList);

        _castMemberRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCastMembersIds);

        var action = () => _useCase.Handle(input, CancellationToken.None);

        await action.Should()
                    .ThrowAsync<RelatedAggregateException>()
                    .WithMessage($"Related castMember id (or ids) not found: {string.Join(',', invalidCastMemberId)}.");

        _videoRepositoryMock.VerifyAll();
        _castMemberRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(UpdateVideosThrowsWhenInvalidCategoryId))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideosThrowsWhenInvalidCategoryId()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var invalidCategoryId = Guid.NewGuid();

        var examplesCategoriesIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        var inputInvalidIdsList = examplesCategoriesIds.Concat(new List<Guid> { invalidCategoryId }).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, categoriesIds: inputInvalidIdsList);

        _categoryRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesCategoriesIds);

        var action = () => _useCase.Handle(input, CancellationToken.None);

        await action.Should()
                    .ThrowAsync<RelatedAggregateException>()
                    .WithMessage($"Related category id (or ids) not found: {string.Join(',', invalidCategoryId)}.");

        _videoRepositoryMock.VerifyAll();
        _categoryRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(UpdateVideosThrowsWhenInvalidGenreId))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideosThrowsWhenInvalidGenreId()
    {
        var exampleVideo = _fixture.GetValidVideo();

        var invalidGenreId = Guid.NewGuid();

        var examplesGenresIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToList();

        var inputInvalidIdsList = examplesGenresIds.Concat(new List<Guid> { invalidGenreId }).ToList();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, inputInvalidIdsList);

        _genreRepositoryMock.Setup(x =>
            x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())
        ).ReturnsAsync(examplesGenresIds);

        var action = () => _useCase.Handle(input, CancellationToken.None);

        await action.Should()
                    .ThrowAsync<RelatedAggregateException>()
                    .WithMessage($"Related genre id (or ids) not found: {string.Join(',', invalidGenreId)}.");

        _videoRepositoryMock.VerifyAll();
        _genreRepositoryMock.VerifyAll();

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(UpdateVideoThrowsWhenVideoNotFound))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoThrowsWhenVideoNotFound()
    {

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.IsAny<Guid>(),
                                             It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Video not found"));

        var input = _fixture.CreateValidInput(Guid.NewGuid());

        var action = () => _useCase.Handle(input, CancellationToken.None);

        await action.Should()
                    .ThrowAsync<NotFoundException>()
                    .WithMessage("Video not found");

        _videoRepositoryMock.Verify(x => x.Update(It.IsAny<DomainEntities.Video>(),
                                                  It.IsAny<CancellationToken>()),
                       Times.Never);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(UpdateVideoWithBannerWhenVideoHasNoBanner))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithBannerWhenVideoHasNoBanner()
    {
        var exampleVideo = _fixture.GetValidVideo();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, banner:_fixture.GetValidImageFileInput());
        var bannerPath = $"storage/banner.{input.Banner!.Extension}";


        _storageServiceMock.Setup(x=>x.Upload(
                It.Is<string>(name => name == StorageFileName.Create(exampleVideo.Id,nameof(exampleVideo.Banner),input.Banner!.Extension)),
                It.IsAny<MemoryStream>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(bannerPath);


        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.BannerFileUrl.Should().Be(bannerPath);
        

        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Banner!.Path == bannerPath ),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.VerifyAll();


        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(UpdateVideoWithBannerKeepBannerWhenReceiveNull))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithBannerKeepBannerWhenReceiveNull()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>()
                                             )
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, banner: null);

        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.BannerFileUrl.Should().Be(exampleVideo.Banner!.Path);


        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Banner!.Path == exampleVideo.Banner!.Path),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x=> x.Upload(It.IsAny<string>(),It.IsAny<MemoryStream>(),It.IsAny<CancellationToken>()),Times.Never);    

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(UpdateVideoWithThumbWhenVideoHasNoThumb))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithThumbWhenVideoHasNoThumb()
    {
        var exampleVideo = _fixture.GetValidVideo();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, thumb: _fixture.GetValidImageFileInput());
        var path = $"storage/thumb.{input.Thumb!.Extension}";


        _storageServiceMock.Setup(x => x.Upload(
                It.Is<string>(name => name == StorageFileName.Create(exampleVideo.Id, nameof(exampleVideo.Thumb), input.Thumb!.Extension)),
                It.IsAny<MemoryStream>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(path);


        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.ThumbFileUrl.Should().Be(path);


        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Thumb!.Path == path),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.VerifyAll();


        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(UpdateVideoWithThumbKeepThumbWhenReceiveNull))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithThumbKeepThumbWhenReceiveNull()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>()
                                             )
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, thumb: null);

        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.ThumbFileUrl.Should().Be(exampleVideo.Thumb!.Path);


        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.Thumb!.Path == exampleVideo.Thumb!.Path),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Upload(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(UpdateVideoWithThumbHalfWhenVideoHasNoThumbHalf))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithThumbHalfWhenVideoHasNoThumbHalf()
    {
        var exampleVideo = _fixture.GetValidVideo();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, thumbHalf: _fixture.GetValidImageFileInput());
        var path = $"storage/thumb-half.{input.ThumbHalf!.Extension}";


        _storageServiceMock.Setup(x => x.Upload(
                It.Is<string>(name => name == StorageFileName.Create(exampleVideo.Id, nameof(exampleVideo.ThumbHalf), input.ThumbHalf!.Extension)),
                It.IsAny<MemoryStream>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(path);


        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.ThumbHalfFileUrl.Should().Be(path);


        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.ThumbHalf!.Path == path),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.VerifyAll();


        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }


    [Fact(DisplayName = nameof(UpdateVideoWithThumbKeepThumbWhenReceiveNull))]
    [Trait("Application", "UpdateVideo - UseCases")]
    public async Task UpdateVideoWithThumbHalfKeepThumbHalfWhenReceiveNull()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        _videoRepositoryMock.Setup(x => x.Get(
                                             It.Is<Guid>(y => y == exampleVideo.Id),
                                             It.IsAny<CancellationToken>()
                                             )
        ).ReturnsAsync(exampleVideo);


        var input = _fixture.CreateValidInput(exampleVideo.Id, thumbHalf: null);

        var output = await _useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating.ToStringSignal());
        output.CreatedAt.Should().NotBe(default);
        output.ThumbHalfFileUrl.Should().Be(exampleVideo.ThumbHalf!.Path);


        _videoRepositoryMock.Verify(repository =>
        repository.Update(It.Is<DomainEntities.Video>(Video => Video.Id == input.VideoId &&
                                                      Video.Title == input.Title &&
                                                      Video.Description == input.Description &&
                                                      Video.Rating == input.Rating &&
                                                      Video.YearLaunched == input.YearLaunched &&
                                                      Video.Opened == input.Opened &&
                                                      Video.Published == input.Published &&
                                                      Video.Duration == input.Duration &&
                                                      Video.ThumbHalf!.Path == exampleVideo.ThumbHalf!.Path),
                          It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Upload(It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
        _videoRepositoryMock.VerifyAll();

    }
}
