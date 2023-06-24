﻿using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.UnitTests.Common;
using System;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.Entity.Video;

[CollectionDefinition(nameof(VideoTestFixture))]
public class VideoTestFixtureCollection : ICollectionFixture<VideoTestFixture>
{
}

public class VideoTestFixture : BaseFixture
{
    public DomainEntity.Video GetValidVideo()
        => new(
            GetValidTitle(),
            GetValidDescription(),
            GetValidYearLaunched(),
            GetRandomBoolean(),
            GetRandomBoolean(),
            GetValidDuration(),
            GetRandomRating()

            );
    public Rating GetRandomRating()
    {
        var enumValues = Enum.GetValues<Rating>();
        return enumValues[new Random().Next(enumValues.Length)];
    }

    public string GetValidImagePath()
        => Faker.Image.PlaceImgUrl();

    public string GetValidDescription()
     => Faker.Commerce.ProductDescription();

    public int GetValidDuration()
        => new Random().Next(100, 300);

    public string GetValidTitle()
        => Faker.Lorem.Letter(100);

    public int GetValidYearLaunched()
        => Faker.Date.BetweenDateOnly(new DateOnly(1960, 1, 1), new DateOnly(2023, 1, 1)).Year;

    public string GetTooLongTitle()
        => Faker.Lorem.Letter(400);

    public string GetTooLongDescription()
        => Faker.Lorem.Letter(4001);

    public Media GetValidMedia()
        => new (GetValidMediaPath());

    public string GetValidMediaPath()
    {
        var exampleMediasPath = new string[]
        {
            "https://www.googlestorage.com/medias/1",
            "https://www.googlestorage.com/medias/2",
            "https://www.googlestorage.com/medias/3",
            "https://www.googlestorage.com/medias/4",
        };
        return exampleMediasPath[new Random().Next(exampleMediasPath.Length)];  
    }
}
