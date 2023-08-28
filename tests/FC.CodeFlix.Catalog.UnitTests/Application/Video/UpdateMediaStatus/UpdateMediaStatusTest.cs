using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UpdateMediaStatus;

[Collection(nameof(UpdateMediaStatusTestFixture))]
public class UpdateMediaStatusTest
{
    private readonly UpdateMediaStatusTestFixture _fixture;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly UseCase.UpdateMediaStatus _usecase;
    private readonly Mock<IVideoRepository> _videoRepository;
    
    public UpdateMediaStatusTest(UpdateMediaStatusTestFixture fixture)
    {
        _fixture = fixture;
        _unitOfWork =new Mock<IUnitOfWork>();
        _videoRepository = new Mock<IVideoRepository>();
        _usecase = new UseCase.UpdateMediaStatus(
            _videoRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<UseCase.UpdateMediaStatus>>());
    }

    [Fact(DisplayName = nameof(HandleWhenSucceededEncoding))]
    [Trait("Application", "UpdateMediaStatusVideo - Use Cases ")]
    public async Task HandleWhenSucceededEncoding()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();
        var input = _fixture.GetSuccededEncondingInput(exampleVideo.Id);

        _videoRepository.Setup(x => x.Get(exampleVideo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleVideo);

        VideoModelOutput output = await _usecase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(exampleVideo.Id);
        output.Title.Should().Be(exampleVideo.Title);
        output.Description.Should().Be(exampleVideo.Description);
        output.Duration.Should().Be(exampleVideo.Duration);
        output.Published.Should().Be(exampleVideo.Published);
        output.Opened.Should().Be(exampleVideo.Opened);
        output.Rating.Should().Be(exampleVideo.Rating.ToStringSignal());
        output.YearLaunched.Should().Be(exampleVideo.YearLaunched);

        exampleVideo.Media!.Status.Should().Be(MediaStatus.Completed);
        exampleVideo.Media!.EncodedPath.Should().Be(input.EncodedPath);

        _videoRepository.VerifyAll();
        _videoRepository.Verify(x => x.Update(exampleVideo, It.IsAny<CancellationToken>() ), Times.Once);

        _unitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);   
    }


    [Fact(DisplayName = nameof(HandleWhenFailedEncoding))]
    [Trait("Application", "UpdateMediaStatusVideo - Use Cases ")]
    public async Task HandleWhenFailedEncoding()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();
        var input = _fixture.GetFailedEncondingInput(exampleVideo.Id);

        _videoRepository.Setup(x => x.Get(exampleVideo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleVideo);

        VideoModelOutput output = await _usecase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(exampleVideo.Id);
        output.Title.Should().Be(exampleVideo.Title);
        output.Description.Should().Be(exampleVideo.Description);
        output.Duration.Should().Be(exampleVideo.Duration);
        output.Published.Should().Be(exampleVideo.Published);
        output.Opened.Should().Be(exampleVideo.Opened);
        output.Rating.Should().Be(exampleVideo.Rating.ToStringSignal());
        output.YearLaunched.Should().Be(exampleVideo.YearLaunched);

        exampleVideo.Media!.Status.Should().Be(MediaStatus.Error);
        exampleVideo.Media!.EncodedPath.Should().BeNull();
    

        _videoRepository.VerifyAll();
        _videoRepository.Verify(x => x.Update(exampleVideo, It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(HandleWhenInvalidInput))]
    [Trait("Application", "UpdateMediaStatusVideo - Use Cases ")]
    public async Task HandleWhenInvalidInput()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();
        var input = _fixture.GetInvalidStatusEncondingInput(exampleVideo.Id);

        var expectedEncondedPath = exampleVideo.Media!.EncodedPath;
        var expetedStatus = exampleVideo.Media.Status;
        var expectedErrorMessage = "Invalid media status";

        _videoRepository.Setup(x => x.Get(exampleVideo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleVideo);

        var action = async () => await _usecase.Handle(input, CancellationToken.None);


        await action.Should().ThrowAsync<EntityValidationException>().WithMessage(expectedErrorMessage);


        exampleVideo.Media!.Status.Should().Be(expetedStatus);
        exampleVideo.Media!.EncodedPath.Should().Be(expectedEncondedPath);


        _videoRepository.VerifyAll();
        _videoRepository.Verify(x => x.Update(exampleVideo, It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
