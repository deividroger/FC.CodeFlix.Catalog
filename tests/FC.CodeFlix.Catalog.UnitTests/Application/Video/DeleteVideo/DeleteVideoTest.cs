
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.DeleteVideo;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.DeleteVideo;

[Collection(nameof(DeleteVideoTestFixture))]
public class DeleteVideoTest
{
    private readonly DeleteVideoTestFixture _fixture;
    private readonly UseCase.DeleteVideo _useCase;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;

    public DeleteVideoTest(DeleteVideoTestFixture fixture)
    {
        _fixture = fixture;

        _unitOfWorkMock = new();
        _repositoryMock = new();
        _storageServiceMock = new();
        _storageServiceMock = new();

        _useCase = new UseCase.DeleteVideo(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _storageServiceMock.Object);
    }

    [Fact(DisplayName = nameof(DeleteVideo))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideo()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(DeleteVideosWithAllMediasAndClearStorage))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideosWithAllMediasAndClearStorage()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateMedia(_fixture.GetValidMediaPath());
        videoExample.UpdateTrailer(_fixture.GetValidMediaPath());
        
        videoExample.UpdateBanner(_fixture.GetValidImagePath());
        videoExample.UpdateThumb(_fixture.GetValidImagePath());
        videoExample.UpdateThumbHalf(_fixture.GetValidImagePath());


        var filesPaths = new List<string>()
        {
            videoExample.Media!.FilePath,
            videoExample.Trailer!.FilePath,
            videoExample.Banner!.Path,
            videoExample.Thumb!.Path,
            videoExample.ThumbHalf!.Path
        };

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filesPaths.Contains(filePath)),
            It.IsAny<CancellationToken>()),
            Times.Exactly(5));

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5));
    }


    [Fact(DisplayName = nameof(DeleteVideoWithOnlyTrailerAndClearStorageOnlyTrailer))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithOnlyTrailerAndClearStorageOnlyTrailer()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateTrailer(_fixture.GetValidMediaPath());

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filePath == videoExample.Trailer!.FilePath),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = nameof(DeleteVideoWithOnlyBannerAndClearStorageOnlyBanner))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithOnlyBannerAndClearStorageOnlyBanner()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateBanner(_fixture.GetValidImagePath());

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filePath == videoExample.Banner!.Path),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact(DisplayName = nameof(DeleteVideoWithOnlyThumbAndClearStorageOnlyThumb))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithOnlyThumbAndClearStorageOnlyThumb()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateThumb(_fixture.GetValidImagePath());

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filePath == videoExample.Thumb!.Path),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = nameof(DeleteVideoWithOnlyThumbHalfAndClearStorageOnlyThumb))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithOnlyThumbHalfAndClearStorageOnlyThumb()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateThumbHalf(_fixture.GetValidImagePath());

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filePath == videoExample.ThumbHalf!.Path),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact(DisplayName = nameof(DeleteVideoWithOnlyMediaAndClearStorageOnlyMedia))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithOnlyMediaAndClearStorageOnlyMedia()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        videoExample.UpdateMedia(_fixture.GetValidMediaPath());

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(filePath => filePath == videoExample.Media!.FilePath),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = nameof(DeleteVideoWithoutAnyMediaAndDontClearStorage))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithoutAnyMediaAndDontClearStorage()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);

        _repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == videoExample.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(
            x => x.Delete(It.Is<DomainEntity.Video>(x => x.Id == videoExample.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);


        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact(DisplayName = nameof(ThrowsNotFoundExceptionWhenVideoNotFound))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task ThrowsNotFoundExceptionWhenVideoNotFound()
    {
        var input = _fixture.GetValidInput();

        _repositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Video not found"));

       var action = () =>  _useCase.Handle(input, CancellationToken.None);

       await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Video not found");

        _repositoryMock.Verify(
            x => x.Delete(
                It.IsAny<DomainEntity.Video>(),
                It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
