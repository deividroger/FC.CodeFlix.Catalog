using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Enum;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;

public abstract class VideoTestFixtureBase: BaseFixture
{
    public Video GetValidVideo()
       => new(
           GetValidTitle(),
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
        video.UpdateThumb( GetValidImagePath());
        video.UpdateBanner( GetValidImagePath());
        video.UpdateThumbHalf( GetValidMediaPath());

        video.UpdateMedia( GetValidMediaPath());
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

        return video;
    }

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
        => new(GetValidMediaPath());

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

    public FileInput GetValidImageFileInput()
    {
        var exampleStream = new MemoryStream(Encoding.ASCII.GetBytes("test"));
        var fileInput = new FileInput("jpg", exampleStream);
        return fileInput;
    }

    public FileInput GetValidMediaFileInput()
    {
        var exampleStream = new MemoryStream(Encoding.ASCII.GetBytes("test"));
        var fileInput = new FileInput("mp4", exampleStream);
        return fileInput;
    }
}
