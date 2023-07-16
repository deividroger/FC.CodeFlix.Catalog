using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.Validation;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.CreateVideo;

[Collection(nameof(CreateVideoTestFixture))]
public class CreateVideoTest
{
    private readonly CreateVideoTestFixture _fixture;

    public CreateVideoTest(CreateVideoTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateVideo))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideo()
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>()
            );

        var input = _fixture.CreateValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);


        repositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);

    }

    [Fact(DisplayName = nameof(CreateVideoWithAllImages))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithAllImages()
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var storageServiceMock = new Mock<IStorageService>();

        var expectedBannerName = $"banner.jpg";
        var expectedThumbName = $"thumb.jpg";
        var expectedThumbHalfName = $"thumbHalf.jpg";


        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-banner.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedBannerName);

        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-thumb.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedThumbName);

        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-thumb-half.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedThumbHalfName);

        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInputWillAllImages();

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating &&
                video.Thumb!.Path == expectedThumbName &&
                video.ThumbHalf!.Path == expectedThumbHalfName &&
                video.Banner!.Path == expectedBannerName

            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        storageServiceMock.VerifyAll();

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);

        output.Thumb.Should().Be(expectedThumbName);
        output.ThumbHalf.Should().Be(expectedThumbHalfName);
        output.Banner.Should().Be(expectedBannerName);

    }

    [Fact(DisplayName = nameof(ThrowsExceptionWhenUploadErrorCases))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task ThrowsExceptionWhenUploadErrorCases()
    {
        var storageServiceMock = new Mock<IStorageService>();

        storageServiceMock.Setup(x => x.Upload(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Error on upload"));

        var useCase = new UseCase.CreateVideo(
            Mock.Of<IVideoRepository>(),
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            Mock.Of<IUnitOfWork>(),
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInputWillAllImages();

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Error on upload");

        storageServiceMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(ThrowsExceptionWhenUploadErrorCases))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task ThrowsExceptionaAndRollbackUploadInErrorCases()
    {
        var storageServiceMock = new Mock<IStorageService>();

        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-banner.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync("banner.jpg");

        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-thumb.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync("thumb.jpg");

        storageServiceMock.Setup(x => x.Upload(
            It.Is<string>(x => x.EndsWith("-thumb-half.jpg")),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Error on upload"));


        var useCase = new UseCase.CreateVideo(
            Mock.Of<IVideoRepository>(),
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            Mock.Of<IUnitOfWork>(),
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInputWillAllImages();

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Error on upload");

        storageServiceMock.Verify(x => 
            x.Delete(It.Is<string>(y => y == "banner.jpg" || y == "thumb.jpg"),It.IsAny<CancellationToken>())
            ,Times.Exactly(2));


        storageServiceMock.VerifyAll();
    }

    [Fact(DisplayName = nameof(CreateVideoWithThumb))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithThumb()
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var storageServiceMock = new Mock<IStorageService>();

        var expectedThumbName = $"thumb.jpg";

        storageServiceMock.Setup(x => x.Upload(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedThumbName);

        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInput(thumb: _fixture.GetValidImageFileInput());

        var output = await useCase.Handle(input, CancellationToken.None);

        storageServiceMock.VerifyAll();

        repositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);
        output.Thumb.Should().Be(expectedThumbName);

    }

    [Fact(DisplayName = nameof(CreateVideoWithThumbHalf))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithThumbHalf()
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var storageServiceMock = new Mock<IStorageService>();

        var expectedThumbHalfName = $"thumb-half.jpg";

        storageServiceMock.Setup(x => x.Upload(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedThumbHalfName);

        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInput(thumbHalf: _fixture.GetValidImageFileInput());

        var output = await useCase.Handle(input, CancellationToken.None);

        storageServiceMock.VerifyAll();
        repositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);
        output.ThumbHalf.Should().Be(expectedThumbHalfName);

    }

    [Fact(DisplayName = nameof(CreateVideoWithBanner))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithBanner()
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var storageServiceMock = new Mock<IStorageService>();

        var expectedBannerName = $"banner.jpg";

        storageServiceMock.Setup(x => x.Upload(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedBannerName);

        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            storageServiceMock.Object
            );

        var input = _fixture.CreateValidInput(banner: _fixture.GetValidImageFileInput());

        var output = await useCase.Handle(input, CancellationToken.None);

        storageServiceMock.VerifyAll();
        repositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);
        output.Thumb.Should().BeNullOrEmpty();
        output.Banner.Should().Be(expectedBannerName);

    }

    [Fact(DisplayName = nameof(CreateVideoWithCategoriesIds))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithCategoriesIds()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var exampleCategoriesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();

        categoryRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(exampleCategoriesIds);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());



        var input = _fixture.CreateValidInput(exampleCategoriesIds);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);

        output.CategoriesIds.Should().BeEquivalentTo(exampleCategoriesIds);

        videoRepositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating &&
                video.Categories.All(categoryId => exampleCategoriesIds.Contains(categoryId))
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        categoryRepositoryMock.VerifyAll();

    }


    [Fact(DisplayName = nameof(ThrowsWhenCategoryIdInvalid))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task ThrowsWhenCategoryIdInvalid()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();

        var exampleCategoriesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();
        var removedCategoryId = exampleCategoriesIds[2];

        categoryRepositoryMock.Setup(x =>
                x.GetIdsListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategoriesIds.FindAll(x => x != removedCategoryId).AsReadOnly());

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());



        var input = _fixture.CreateValidInput(exampleCategoriesIds);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should()
            .ThrowAsync<RelatedAggregateException>()
            .WithMessage($"Related category id (or ids) not found: {removedCategoryId}.");

        categoryRepositoryMock.VerifyAll();
    }


    [Fact(DisplayName = nameof(CreateVideoWithGenresIds))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithGenresIds()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        var genresRepositoryMock = new Mock<IGenreRepository>();

        var examplesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();

        genresRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(examplesIds);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            genresRepositoryMock.Object,
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());

        var input = _fixture.CreateValidInput(genresIds: examplesIds);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);

        output.GenresIds.Should().BeEquivalentTo(examplesIds);
        output.CategoriesIds.Should().BeEmpty();

        videoRepositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating &&
                video.Genres.All(id => examplesIds.Contains(id))
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        genresRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(ThrowsWhenInvalidGenreId))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task ThrowsWhenInvalidGenreId()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        var genresRepositoryMock = new Mock<IGenreRepository>();

        var examplesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();
        var removedId = examplesIds[2];

        genresRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(examplesIds.FindAll(id => id != removedId));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            genresRepositoryMock.Object,
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());

        var input = _fixture.CreateValidInput(genresIds: examplesIds);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should()
            .ThrowAsync<RelatedAggregateException>()
            .WithMessage($"Related genre id (or ids) not found: {removedId}.");

        genresRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(CreateVideoWithCastMembersIds))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task CreateVideoWithCastMembersIds()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        var genresRepositoryMock = new Mock<IGenreRepository>();
        var castMemberRepositoryMock = new Mock<ICastMemberRepository>();

        var examplesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();

        castMemberRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(examplesIds);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            genresRepositoryMock.Object,
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());

        var input = _fixture.CreateValidInput(castMembersIds: examplesIds);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Title.Should().Be(input.Title);
        output.Description.Should().Be(input.Description);
        output.YearLaunched.Should().Be(input.YearLaunched);
        output.Published.Should().Be(input.Published);
        output.Opened.Should().Be(input.Opened);
        output.Duration.Should().Be(input.Duration);
        output.Rating.Should().Be(input.Rating);
        output.CreatedAt.Should().NotBe(default);

        output.GenresIds.Should().BeEmpty();
        output.CategoriesIds.Should().BeEmpty();

        output.CastMembersIds.Should().BeEquivalentTo(examplesIds);

        videoRepositoryMock.Verify(x => x.Insert(It.Is<DomainEntity.Video>(
            video =>
            video.Title == input.Title &&
                video.Description == input.Description &&
                video.YearLaunched == input.YearLaunched &&
                video.Id != Guid.Empty &&
                video.Published == input.Published &&
                video.Opened == input.Opened &&
                video.Duration == input.Duration &&
                video.Rating == input.Rating &&
                video.CastMembers.All(id => examplesIds.Contains(id))
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(X => X.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        castMemberRepositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(ThrowsWhenInvalidCastMemberId))]
    [Trait("Application", "Create Video - Use Cases")]
    public async Task ThrowsWhenInvalidCastMemberId()
    {
        var videoRepositoryMock = new Mock<IVideoRepository>();
        var categoryRepositoryMock = new Mock<ICategoryRepository>();
        var genresRepositoryMock = new Mock<IGenreRepository>();
        var castMemberRepositoryMock = new Mock<ICastMemberRepository>();

        var examplesIds = Enumerable.Range(1, 5)
                                             .Select(_ => Guid.NewGuid())
                                             .ToList();
        var removedId = examplesIds[2];

        castMemberRepositoryMock.Setup(x => x.GetIdsListByIds(
            It.IsAny<List<Guid>>(),
            It.IsAny<CancellationToken>()
            )).ReturnsAsync(examplesIds.FindAll(x => x != removedId));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            videoRepositoryMock.Object,
            categoryRepositoryMock.Object,
            genresRepositoryMock.Object,
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());

        var input = _fixture.CreateValidInput(castMembersIds: examplesIds);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should()
            .ThrowAsync<RelatedAggregateException>()
            .WithMessage($"Related castMember id (or ids) not found: {removedId}.");

        castMemberRepositoryMock.VerifyAll();

    }

    [Theory(DisplayName = nameof(CreateVideoThrowsWithInvalidInput))]
    [Trait("Application", "Create Video - Use Cases")]
    [ClassData(typeof(CreateVideoTestDataGenerator))]
    public async Task CreateVideoThrowsWithInvalidInput(CreateVideoInput input, string expectedValidationError)
    {
        var repositoryMock = new Mock<IVideoRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var useCase = new UseCase.CreateVideo(
            repositoryMock.Object,
            Mock.Of<ICategoryRepository>(),
            Mock.Of<IGenreRepository>(),
            Mock.Of<ICastMemberRepository>(),
            unitOfWorkMock.Object,
            Mock.Of<IStorageService>());

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        (await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("There are validation errors"))
            .Which.Errors.Should().BeEquivalentTo(new List<ValidationError>()
                {
                    new ValidationError(expectedValidationError)
                }
            );

        repositoryMock.Verify(x => x.Insert(It.IsAny<DomainEntity.Video>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}