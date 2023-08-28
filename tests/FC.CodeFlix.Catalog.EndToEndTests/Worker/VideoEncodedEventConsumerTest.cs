using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;
using FC.CodeFlix.Catalog.Infra.Messaging.Dtos;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Worker;

[Collection(nameof(VideoBaseFixture))]
public class VideoEncodedEventConsumerTest : IDisposable
{
    private readonly VideoBaseFixture _fixture;

    public VideoEncodedEventConsumerTest(VideoBaseFixture fixture) => _fixture = fixture;

    [Fact(DisplayName = nameof(EncodingSuccessedEventReceived))]
    [Trait("End2End/Worker", "VideoEncoded - Event Handler")]
    public async Task EncodingSuccessedEventReceived()
    {
        var exampleVideos = _fixture.GetVideoCollection(5);
        await _fixture.VideoPersistence.InsertList(exampleVideos);
        var video = exampleVideos[2];

        var encodedFilePath = _fixture.GetValidMediaPath();

        var exampleEvent = new VideoEncodedMessageDTO
        {
            Video = new VideoEncodedMetadataDTO
            {
                EncodedVideoFolder = encodedFilePath,
                FilePath = video.Media!.FilePath,
                ResourceId = video.Id.ToString(),
            }
        };
        _fixture.PublishMessageToRabbitMQ(exampleEvent);

        await Task.Delay(3000); //wait to message arrive in queue

        var videoFromDb = await _fixture.VideoPersistence.GetById(video.Id);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Media!.Status.Should().Be(MediaStatus.Completed);
        videoFromDb!.Media!.EncodedPath.Should().Be(exampleEvent.Video.FullEncodedVideoFilePath);

        var (@event, count) = _fixture.ReadMessageFromRabbitMQ<object>();

        @event.Should().BeNull();
        count.Should().Be(0);
    }



    [Fact(DisplayName = nameof(EncodingFailedEventReceived))]
    [Trait("End2End/Worker", "VideoEncoded - Event Handler")]
    public async Task EncodingFailedEventReceived()
    {
        var exampleVideos = _fixture.GetVideoCollection(5);
        await _fixture.VideoPersistence.InsertList(exampleVideos);
        var video = exampleVideos[2];


        var exampleEvent = new VideoEncodedMessageDTO
        {
            Message = new VideoEncodedMetadataDTO
            {
                FilePath = video.Media!.FilePath,
                ResourceId = video.Id.ToString()
            },
            Error = "There was an error on processing the video"
        };
        _fixture.PublishMessageToRabbitMQ(exampleEvent);

        await Task.Delay(800); //wait to message arrive in queue

        var videoFromDb = await _fixture.VideoPersistence.GetById(video.Id);

        videoFromDb.Should().NotBeNull();

        videoFromDb!.Media!.Status.Should().Be(MediaStatus.Error);
        videoFromDb!.Media!.EncodedPath.Should().BeNull();

        var (@event, count) = _fixture.ReadMessageFromRabbitMQ<object>();

        @event.Should().BeNull();
        count.Should().Be(0);
    }


    [Fact(DisplayName = nameof(InvalidMessageEventReceived))]
    [Trait("End2End/Worker", "VideoEncoded - Event Handler")]
    public async Task InvalidMessageEventReceived()
    {
        var exampleVideos = _fixture.GetVideoCollection(5);
        await _fixture.VideoPersistence.InsertList(exampleVideos);
        var video = exampleVideos[2];


        var exampleEvent = new VideoEncodedMessageDTO 
        {
            Message = new VideoEncodedMetadataDTO
            {
                FilePath = _fixture.GetValidMediaPath(),
                ResourceId = Guid.NewGuid().ToString()
            },
            Error = "There was an error on processing the video"
        };
        _fixture.PublishMessageToRabbitMQ(exampleEvent);

        await Task.Delay(800); //wait to message arrive in queue


        var (@event, count) = _fixture.ReadMessageFromRabbitMQ<object>();

        @event.Should().BeNull();
        count.Should().Be(0);
    }


    public void Dispose()
    {
        _fixture.CleanPersistence();
        _fixture.PurgeRabbitMQQueues();
    }
}
