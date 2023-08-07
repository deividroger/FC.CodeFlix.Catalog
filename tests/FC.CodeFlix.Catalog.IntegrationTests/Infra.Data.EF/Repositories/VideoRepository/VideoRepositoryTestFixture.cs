using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.IntegrationTests.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.VideoRepository;

[CollectionDefinition(nameof(VideoRepositoryTestFixture))]
public class VideoRepositoryTestFixtureCollection : ICollectionFixture<VideoRepositoryTestFixture>
{
}

public class VideoRepositoryTestFixture
    : BaseFixture
{

    public List<Video> GetExampleVideosList(int count = 10)
        => Enumerable.Range(1, count)
            .Select(_ => GetExampleVideo())
            .ToList();

    public List<Video> GetExampleVideosListByTitles(List<string> titles)
        => titles.Select(title => GetExampleVideo(title: title)).ToList();

    public Video GetExampleVideo(string? title = null)
       => new(
               title ?? GetValidTitle(),
               GetValidDescription(),
               GetValidYearLaunched(),
               GetRandomBoolean(),
               GetRandomBoolean(),
               GetValidDuration(),
               GetRandomRating()
           );

    public Video GetValidVideoWithAllProperties()
    {
        var video = new Video(
               GetValidTitle(),
               GetValidDescription(),
               GetValidYearLaunched(),
               GetRandomBoolean(),
               GetRandomBoolean(),
               GetValidDuration(),
               GetRandomRating()

               );
        video.UpdateThumb(GetValidImagePath());
        video.UpdateBanner(GetValidImagePath());
        video.UpdateThumbHalf(GetValidMediaPath());

        video.UpdateMedia(GetValidMediaPath());
        video.UpdateTrailer(GetValidImagePath());

        var random = new Random();
        Enumerable.Range(1, random.Next(2, 5))
            .ToList()
            .ForEach(_ => video.AddCastMember(Guid.NewGuid()));

        Enumerable.Range(1, random.Next(2, 5))
            .ToList()
            .ForEach(_ => video.AddGenre(Guid.NewGuid()));

        Enumerable.Range(1, random.Next(2, 5))
            .ToList()
            .ForEach(_ => video.AddCastMember(Guid.NewGuid()));

        video.UpdateAsEncoded(GetValidMediaPath());

        return video;
    }

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

    public string GetValidImagePath()
        => Faker.Image.PlaceImgUrl();

    public string GetValidDescription()
     => Faker.Commerce.ProductDescription();

    public static int GetValidDuration()
        => new Random().Next(100, 300);

    public string GetValidTitle()
        => Faker.Lorem.Letter(100);

    public int GetValidYearLaunched()
            => Faker.Date.BetweenDateOnly(new DateOnly(1960, 1, 1), new DateOnly(2023, 1, 1)).Year;

    public static bool GetRandomBoolean()
            => new Random().NextDouble() < 0.5;

    public static Rating GetRandomRating()
    {
        var enumValues = Enum.GetValues<Rating>();
        return enumValues[new Random().Next(enumValues.Length)];
    }

    public List< CastMember> GetRandomCastMemberList()
     => Enumerable.Range(1, 5)
        .Select(_ => GetExampleCastMember())
        .ToList();

    public List<Category> GetRandomCategoryList()
        => Enumerable.Range(1, 5)
            .Select(_ => GetValidCategory())
            .ToList();

    public List<Genre> GetRandomGenreList()
        => Enumerable.Range(1, 5)
            .Select(_ => GetExampleGenre())
            .ToList();

    public Category GetValidCategory()
        => new(GetValidCategoryName(), GetValidCategoryDescription());


    public Genre GetExampleGenre()
        => new(GetValidGenreName(), true);
    

    public CastMember GetExampleCastMember()
     => new(GetValidCastMemberName(), GetRandomCastMemberType());

    public string GetValidCastMemberName()
        => Faker.Name.FullName();

    public static CastMemberType GetRandomCastMemberType()
        => (CastMemberType)new Random().Next(1, 2);

    public string GetValidCategoryName()
    {
        var categoryName = "";

        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];

        if (categoryName.Length > 255)
            categoryName = categoryName[..255];

        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();


        if (categoryDescription.Length > 10_000)
            categoryDescription = categoryDescription[..10_000];


        return categoryDescription;
    }


    public string GetValidGenreName()
     => Faker.Commerce.Categories(1)[0];

    public static List<Video> CloneListOrdered(List<Video> videoList, SearchInput input)
    {
        var listClone = new List<Video>(videoList);

        var orderedEnumerable = (input.OrderBy.ToLower(), input.Order) switch
        {
            ("title", SearchOrder.ASC) => listClone.OrderBy(x => x.Title)
                .ThenBy(x => x.Id),
            ("title", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Title)
                .ThenByDescending(x => x.Id),

            ("id", SearchOrder.ASC) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Id),

            ("createdat", SearchOrder.ASC) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.DESC) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Title)
                   .ThenBy(x => x.Id),
        };

        return orderedEnumerable.ToList();
    }
}
