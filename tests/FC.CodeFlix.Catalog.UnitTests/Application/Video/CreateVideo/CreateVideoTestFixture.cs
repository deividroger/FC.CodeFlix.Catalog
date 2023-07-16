﻿using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;
using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.CreateVideo;


[CollectionDefinition(nameof(CreateVideoTestFixture))]
public class CreateVideoTestFixtureCollection : ICollectionFixture<CreateVideoTestFixture>
{
}

public class CreateVideoTestFixture : VideoTestFixtureBase
{
    public CreateVideoInput CreateValidInput(
        List<Guid>? categoriesIds = null,
        List<Guid>? genresIds = null,
        List<Guid>? castMembersIds = null,
        FileInput? thumb = null,
        FileInput? banner = null,
        FileInput? thumbHalf = null
        )
        => new(
                GetValidTitle(),
                GetValidDescription(),
                GetValidYearLaunched(),
                GetRandomBoolean(),
                GetRandomBoolean(),
                GetValidDuration(),
                GetRandomRating(),
                categoriesIds,
                genresIds,
                castMembersIds,
                thumb,
                banner,
                thumbHalf
                );

    public CreateVideoInput CreateValidInputWillAllImages()
       => new(
               GetValidTitle(),
               GetValidDescription(),
               GetValidYearLaunched(),
               GetRandomBoolean(),
               GetRandomBoolean(),
               GetValidDuration(),
               GetRandomRating(),
                null,
                null,
                null,
               GetValidImageFileInput(),
               GetValidImageFileInput(),
               GetValidImageFileInput()
               );

}