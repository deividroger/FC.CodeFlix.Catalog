using FC.CodeFlix.Catalog.Api.ApiModels.Video;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.Events;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common;
using FC.CodeFlix.Catalog.Infra.Messaging.JsonPolicies;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;

[CollectionDefinition(nameof(VideoBaseFixture))]
public class VideoBaseFixtureCollection : ICollectionFixture<VideoBaseFixture>
{
}

public class VideoBaseFixture : GenreBaseFixture
{
    public VideoPersistence VideoPersistence { get; private set; }
    public  CastMemberPersistence CastMemberPersistence { get; private set; }

    private readonly string VideoCreatedQueue = "video.created.queue";

    private readonly string RoutingKey = "video.created";

    public VideoBaseFixture() : base()
    {
        VideoPersistence = new VideoPersistence(DbContext);
        CastMemberPersistence = new CastMemberPersistence(DbContext);
    }


    public void SetupRabbitMQ()
    {
        var channel = WebAppFactory.RabbitMQChannel!;
        var exchange = WebAppFactory.RabbitMQConfiguration.Exchange;
        channel.ExchangeDeclare(exchange, ExchangeType.Direct, true, false);
        channel.QueueDeclare(VideoCreatedQueue, true, false, false);
        channel.QueueBind(VideoCreatedQueue, exchange, RoutingKey);

    }

    public void TearDownRabbitQA()
    {
        var channel = WebAppFactory.RabbitMQChannel!;
        var exchange = WebAppFactory.RabbitMQConfiguration.Exchange;

        channel.QueueUnbind(VideoCreatedQueue, exchange, RoutingKey);
        channel.QueueDelete(VideoCreatedQueue, false, false);
        channel.ExchangeDelete(exchange, false);
    }

    public (VideoUploadedEvent?, uint) ReadMessageFromRabbitMQ()
    {
        var consumingResult = WebAppFactory.RabbitMQChannel!.BasicGet(VideoCreatedQueue, true);

        var rawMessage = consumingResult.Body.ToArray();
        var stringMessage = Encoding.UTF8.GetString(rawMessage);

        var jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = new JsonSnakeCasePolicy()
        };

        var @event = JsonSerializer.Deserialize<VideoUploadedEvent>(stringMessage, jsonOptions);

        return (@event!, consumingResult.MessageCount);  
    }

    public CreateVideoApiInput GetBasicCreateVideoInput()
    {
        return new CreateVideoApiInput()
        {
            Description = GetValidDescription(),
            Duration = GetValidDuration(),
            Opened = GetRandomBoolean(),
            Published = GetRandomBoolean(),
            Title = GetValidTitle(),
            YearLaunched = GetValidYearLaunched(),
            Rating = GetRandomRating().ToStringSignal(),
        };
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
        var fileInput = new FileInput("jpg", exampleStream, "image/jpeg");
        return fileInput;
    }

    public FileInput GetValidMediaFileInput()
    {
        var exampleStream = new MemoryStream(Encoding.ASCII.GetBytes("test"));
        var fileInput = new FileInput("mp4", exampleStream, "video/mp4");
        return fileInput;
    }

    public List<DomainEntity.Video> GetVideoCollection(int quantity = 10)
    => Enumerable.Range(1, quantity)
            .Select(_ => GetValidVideoWithAllProperties())
            .ToList();

    public List<DomainEntity.Video> GetVideoCollection(IEnumerable<string> titles)
    => titles
            .Select(title => GetValidVideoWithAllProperties(title))
            .ToList();


    public DomainEntity.Video GetValidVideoWithAllProperties(string? title = null)
    {
        var video = new DomainEntity.Video(
               title ?? GetValidTitle(),
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



        return video;
    }

    public List<DomainEntity.Video> CloneVideoListOrdered(List<DomainEntity.Video> examplesVideos, string orderBy, SearchOrder order)
    {
        var listClone = new List<DomainEntity.Video>(examplesVideos);

        var orderedEnumerable = (orderBy.ToLower(), order) switch
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


    #region CastMember
    public string GetValidName()
   => Faker.Name.FullName();

    public DomainEntity.CastMember GetExampleCastMember()
        => new(GetValidName(), GetRandomCastMemberType());

    public CastMemberType GetRandomCastMemberType()
        => (CastMemberType)new Random().Next(1, 2);

    public List<DomainEntity.CastMember> GetExampleCastMemberList(int quantity = 10)
        => Enumerable.Range(1, quantity)
            .Select(_ => GetExampleCastMember())
            .ToList();





    #endregion
}
