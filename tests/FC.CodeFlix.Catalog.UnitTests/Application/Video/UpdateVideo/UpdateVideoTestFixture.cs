
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UpdateVideo;

[CollectionDefinition(nameof(UpdateVideoTestFixture))]
public class UpdateVideoTestFixtureCollection : ICollectionFixture<UpdateVideoTestFixture>
{

}

public class UpdateVideoTestFixture : VideoTestFixtureBase
{
    public UseCase.UpdateVideoInput CreateValidInput(
        Guid videoId,
        List<Guid>? genreIds = null,
        List<Guid>? categoriesIds = null,
        List<Guid>? castMemberIds = null,
        FileInput? banner = null,
        FileInput? thumb = null,
        FileInput? thumbHalf = null
        ) 
        => new(videoId,
                    GetValidTitle(),
                    GetValidDescription(),
                    GetValidYearLaunched(),
                    GetRandomBoolean(),
                    GetRandomBoolean(),
                    GetValidDuration(),
                    GetRandomRating(),
                    genreIds,
                    categoriesIds,
                    castMemberIds,
                    banner,
                    thumb,
                    thumbHalf);
}
