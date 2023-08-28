using FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UpdateMediaStatus;


[CollectionDefinition(nameof(UpdateMediaStatusTestFixture))]
public class UpdateMediaStatusTestFixtureCollection: ICollectionFixture<UpdateMediaStatusTestFixture> { }

public class UpdateMediaStatusTestFixture: VideoBaseFixture
{
    public UpdateMediaStatusInput GetSuccededEncondingInput(Guid videoId)
        => new UpdateMediaStatusInput(videoId, 
            MediaStatus.Completed,
            EncodedPath: GetValidMediaPath());

    public UpdateMediaStatusInput GetFailedEncondingInput(Guid videoId)
        => new UpdateMediaStatusInput(videoId,
            MediaStatus.Error,
            ErrorMessage: "There was an error while trying to enconde video.");

    internal UpdateMediaStatusInput GetInvalidStatusEncondingInput(Guid videoId)
    => new UpdateMediaStatusInput(videoId,
            MediaStatus.Processing,
            ErrorMessage: "There was an error while trying to enconde video.");
}
