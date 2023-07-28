
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.GetVideo;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.GetVideo;

[Collection(nameof(GetVideoTestFixture))]
public class GetVideoTest
{
    private readonly GetVideoTestFixture _fixture;

    public GetVideoTest(GetVideoTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName =nameof(Get))]
    [Trait("Application", "GetVideo - UseCases")]
    public async Task Get()
    {
        var exampleVideo = _fixture.GetValidVideo();  

        var repositoryMock = new Mock<IVideoRepository>();

        repositoryMock.Setup(x=> x.Get(It.Is<Guid>(y=> y == exampleVideo.Id),It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleVideo);

        var useCase = new UseCase.GetVideo(repositoryMock.Object);

        var input = new UseCase.GetVideoInput(exampleVideo.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();

        output.Id.Should().Be(exampleVideo.Id);
        output.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(10));
        output.Title.Should().Be(exampleVideo.Title);
        output.Description.Should().Be(exampleVideo.Description);
        output.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        output.Published.Should().Be(exampleVideo.Published);
        output.Opened.Should().Be(exampleVideo.Opened);
        output.Duration.Should().Be(exampleVideo.Duration);
        output.Rating.Should().Be(exampleVideo.Rating.ToStringSignal());


        repositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(GetWithAllProperties))]
    [Trait("Application", "GetVideo - UseCases")]
    public async Task GetWithAllProperties()
    {
        var exampleVideo = _fixture.GetValidVideoWithAllProperties();

        var repositoryMock = new Mock<IVideoRepository>();

        repositoryMock.Setup(x => x.Get(It.Is<Guid>(y => y == exampleVideo.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleVideo);

        var useCase = new UseCase.GetVideo(repositoryMock.Object);

        var input = new UseCase.GetVideoInput(exampleVideo.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();

        output.Id.Should().Be(exampleVideo.Id);
        output.CreatedAt.Should().BeCloseTo(exampleVideo.CreatedAt, TimeSpan.FromSeconds(10));
        output.Title.Should().Be(exampleVideo.Title);
        output.Description.Should().Be(exampleVideo.Description);
        output.YearLaunched.Should().Be(exampleVideo.YearLaunched);
        output.Published.Should().Be(exampleVideo.Published);
        output.Opened.Should().Be(exampleVideo.Opened);
        output.Duration.Should().Be(exampleVideo.Duration);
        output.Rating.Should().Be(exampleVideo.Rating.ToStringSignal());

        output.ThumbFileUrl.Should().Be(exampleVideo.Thumb!.Path);
        output.ThumbHalfFileUrl.Should().Be(exampleVideo.ThumbHalf!.Path);
        output.BannerFileUrl.Should().Be(exampleVideo.Banner!.Path);

        output.VideoFileUrl.Should().Be(exampleVideo.Media!.FilePath);
        output.TrailerFileUrl.Should().Be(exampleVideo.Trailer!.FilePath);

        
        output.Categories!
                      .Select(dto => dto.Id)
                       .ToList().Should().BeEquivalentTo(exampleVideo.Categories);

        output.Genres!
                  .Select(dto => dto.Id)
                   .ToList().Should().BeEquivalentTo(exampleVideo.Genres);

        output.CastMembers!
                  .Select(dto => dto.Id)
                   .ToList().Should().BeEquivalentTo(exampleVideo.CastMembers);

        repositoryMock.VerifyAll();

    }

    [Fact(DisplayName = nameof(ThrowsExceptionWhenNotFound))]
    [Trait("Application", "GetVideo - UseCases")]
    public async Task ThrowsExceptionWhenNotFound()
    {

        var repositoryMock = new Mock<IVideoRepository>();

        repositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Video not Found"));

        var useCase = new UseCase.GetVideo(repositoryMock.Object);

        var input = new UseCase.GetVideoInput(Guid.NewGuid());

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage("Video not Found");


        repositoryMock.VerifyAll();
        
    }
}