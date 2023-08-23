using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.UploadMedias;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UploadMedias;

[Collection(nameof(UploadMediasTestFixture))]
public class UploadMediasTest
{
    private readonly UploadMediasTestFixture _fixture;
    private readonly UseCase.UploadMedias _useCase;
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;

    public UploadMediasTest(UploadMediasTestFixture fixture)
    {
        _fixture = fixture;

        _repositoryMock = new();
        _unitOfWorkMock = new();
        _storageServiceMock = new();

        _useCase = new(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _storageServiceMock.Object);
    }


    [Fact(DisplayName = nameof(UploadMedias))]
    [Trait("Application", "UploadMedias - Use Cases")]
    public async Task UploadMedias()
    {
        var video = _fixture.GetValidVideo();

        var validInput = _fixture.GetValidInput(video.Id);

        var fileNames = new List<string>()
        {
            StorageFileName.Create(video.Id, nameof(video.Media), validInput.VideoFile!.Extension),
            StorageFileName.Create(video.Id, nameof(video.Trailer), validInput.TrailerFile!.Extension),

            StorageFileName.Create(video.Id, nameof(video.Banner), validInput.BannerFile!.Extension),
            StorageFileName.Create(video.Id, nameof(video.Thumb), validInput.ThumbHalfFile!.Extension),
            StorageFileName.Create(video.Id, nameof(video.ThumbHalf), validInput.ThumbHalfFile!.Extension),

        };

        _repositoryMock.Setup(r => r.Get(It.Is<Guid>(x => x == video.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(Guid.NewGuid().ToString());

        await _useCase.Handle(validInput, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _storageServiceMock.Verify(
            x =>
            x.Upload(
                It.Is<string>(x => fileNames.Contains(x)),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
            It.IsAny<CancellationToken>())
            , Times.Exactly(5));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()));
    }

    [Fact(DisplayName = nameof(ClearStorageInUploadErrorCase))]
    [Trait("Application", "UploadMedias - Use Cases")]
    public async Task ClearStorageInUploadErrorCase()
    {
        var video = _fixture.GetValidVideo();

        var validInput = _fixture.GetValidInput(video.Id);

        var videoFileName = StorageFileName.Create(video.Id, nameof(video.Media), validInput.VideoFile!.Extension);
        var trailerFileName = StorageFileName.Create(video.Id, nameof(video.Trailer), validInput.TrailerFile!.Extension);

        var fileNames = new List<string>()
        {
            videoFileName,
            trailerFileName
        };
        var videoStoragePath = $"storage/{videoFileName}";

        _repositoryMock.Setup(r => r.Get(It.Is<Guid>(x => x == video.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.Is<string>(x => x == videoFileName),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(videoStoragePath);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.Is<string>(x => x == trailerFileName),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ThrowsAsync(new Exception("error in upload"));

        var action = () => _useCase.Handle(validInput, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("error in upload");

        _repositoryMock.VerifyAll();
        _storageServiceMock.Verify(
            x =>
            x.Upload(
                It.Is<string>(x => fileNames.Contains(x)),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
            It.IsAny<CancellationToken>())
            , Times.Exactly(2));

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(fileName => fileName == videoStoragePath),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ClearStorageInCommitErrorCases))]
    [Trait("Application", "UploadMedias - Use Cases")]
    public async Task ClearStorageInCommitErrorCases()
    {
        var video = _fixture.GetValidVideo();

        var validInput = _fixture.GetValidInput(video.Id);

        var videoFileName = StorageFileName.Create(video.Id, nameof(video.Media), validInput.VideoFile!.Extension);
        var trailerFileName = StorageFileName.Create(video.Id, nameof(video.Trailer), validInput.TrailerFile!.Extension);

        var bannerFileName = StorageFileName.Create(video.Id, nameof(video.Banner), validInput.BannerFile!.Extension);
        var thumbFileName = StorageFileName.Create(video.Id, nameof(video.Thumb), validInput.ThumbFile!.Extension);
        var thumbHalfFileName = StorageFileName.Create(video.Id, nameof(video.ThumbHalf), validInput.ThumbHalfFile!.Extension);

        var fileNames = new List<string>()
        {
            videoFileName,
            trailerFileName,
            bannerFileName,
            thumbFileName,
            thumbHalfFileName
        };

        var videoStoragePath = $"storage/{videoFileName}";
        var trailerStoragePath = $"storage/{trailerFileName}";

        var bannerStoragePath = $"storage/{bannerFileName}";
        var thumbStoragePath = $"storage/{thumbFileName}";
        var thumbHalfStoragePath = $"storage/{thumbHalfFileName}";

        var filesPathNames = new List<string>()
        {
            videoStoragePath,
            trailerStoragePath,
            bannerStoragePath,
            thumbStoragePath,
            thumbHalfStoragePath
        };

        _repositoryMock.Setup(r => r.Get(It.Is<Guid>(x => x == video.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.Is<string>(x => x == videoFileName),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(videoStoragePath);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.Is<string>(x => x == trailerFileName),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(trailerStoragePath);


        _storageServiceMock.Setup(
               x => x.Upload(
                   It.Is<string>(x => x == bannerFileName),
                   It.IsAny<Stream>(),
                   It.IsAny<string>(),
                   It.IsAny<CancellationToken>())
           ).ReturnsAsync(bannerStoragePath);

        _storageServiceMock.Setup(
                       x => x.Upload(
                           It.Is<string>(x => x == thumbFileName),
                           It.IsAny<Stream>(),
                           It.IsAny<string>(),
                           It.IsAny<CancellationToken>())
         ).ReturnsAsync(thumbStoragePath);

        _storageServiceMock.Setup(
               x => x.Upload(
                   It.Is<string>(x => x == thumbHalfFileName),
                   It.IsAny<Stream>(),
                   It.IsAny<string>(),
                   It.IsAny<CancellationToken>())
        ).ReturnsAsync(thumbHalfStoragePath);


        _unitOfWorkMock.Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("something went wrong with the commit"));

        var action = () => _useCase.Handle(validInput, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("something went wrong with the commit");

        _repositoryMock.VerifyAll();
        _storageServiceMock.Verify(
            x =>
            x.Upload(
                It.Is<string>(x => fileNames.Contains(x)),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
            It.IsAny<CancellationToken>())
            , Times.Exactly(5));


        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(5));

        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(fileName => filesPathNames.Contains(fileName)),
            It.IsAny<CancellationToken>()), Times.Exactly(5));

    }


    [Fact(DisplayName = nameof(CleaOnlyOneFileStorageInCommitErrorCasesIfProvidedOnlyOneFile))]
    [Trait("Application", "UploadMedias - Use Cases")]
    public async Task CleaOnlyOneFileStorageInCommitErrorCasesIfProvidedOnlyOneFile()
    {
        var video = _fixture.GetValidVideo();

        video.UpdateTrailer(_fixture.GetValidMediaPath());
        video.UpdateMedia(_fixture.GetValidMediaPath());

        var validInput = _fixture.GetValidInput(video.Id,
                                                withTrailerFile: false,
                                                withBannerFile: false,
                                                withThumbHalfFile: false,
                                                withThumbFile: false);
        var videoFileName = StorageFileName.Create(video.Id, nameof(video.Media), validInput.VideoFile!.Extension);
        var videoStoragePath = $"storage/{videoFileName}";

        _repositoryMock.Setup(r => r.Get(It.Is<Guid>(x => x == video.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(Guid.NewGuid().ToString());

        _storageServiceMock.Setup(
                x => x.Upload(
                    It.Is<string>(x => x == videoFileName),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())
            ).ReturnsAsync(videoStoragePath);

        _unitOfWorkMock.Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("something went wrong with the commit"));

        var action = () => _useCase.Handle(validInput, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("something went wrong with the commit");

        _repositoryMock.VerifyAll();
        _storageServiceMock.Verify(
            x =>
            x.Upload(
                It.Is<string>(x => x == videoFileName),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
            It.IsAny<CancellationToken>())
            , Times.Once);

        _storageServiceMock.Verify(
            x =>
            x.Upload(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
            It.IsAny<CancellationToken>())
            , Times.Once);


        _storageServiceMock.Verify(x => x.Delete(It.Is<string>(y => y == videoStoragePath),
            It.IsAny<CancellationToken>()), Times.Once);

        _storageServiceMock.Verify(x => x.Delete(It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowsWhenVideoNotFound))]
    [Trait("Application", "UploadMedias - Use Cases")]
    public async Task ThrowsWhenVideoNotFound()
    {
        var video = _fixture.GetValidVideo();

        var validInput = _fixture.GetValidInput(video.Id);

        _repositoryMock.Setup(r => r.Get(It.Is<Guid>(x => x == video.Id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("video not found"));

        var action = () => _useCase.Handle(validInput, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage("video not found");

    }
}
