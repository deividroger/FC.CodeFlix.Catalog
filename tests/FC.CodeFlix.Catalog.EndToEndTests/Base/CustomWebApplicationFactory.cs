using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Messaging.Configuration;
using FC.CodeFlix.Catalog.Infra.Storage.Configuration;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FC.CodeFlix.Catalog.EndToEndTests.Base;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup>, IDisposable
    where TStartup : class
{

    public string VideoCreatedQueue => "video.created.queue";

    public string VideoEncodedRoutingKey => "video.encoded";

    private const string VideoCreatedRoutingKey = "video.created";

    
    public Mock<StorageClient> StorageClient { get; private set; }

    public IModel RabbitMQChannel { get; private set; }

    public RabbitMQConfiguration RabbitMQConfiguration { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("EndToEndTest");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(s => s.ServiceType == typeof(StorageClient));
            services.Remove(descriptor);

            services.AddScoped(sp =>
            {
                StorageClient = new Mock<StorageClient>();
                return StorageClient.Object;
            });


            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            RabbitMQChannel = scope
                                .ServiceProvider
                                .GetService<ChannelManager>()!
                                .GetChannel();
            RabbitMQConfiguration = scope
                                .ServiceProvider
                                .GetService<IOptions<RabbitMQConfiguration>>()!
                                .Value;

            SetupRabbitMQ();

            var context = scope.ServiceProvider.GetService<CodeFlixCatalogDbContext>();
            ArgumentNullException.ThrowIfNull(context);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

        });

        base.ConfigureWebHost(builder);
    }


    public void SetupRabbitMQ()
    {
        var channel = RabbitMQChannel!;
        var exchange = RabbitMQConfiguration.Exchange;
        channel.ExchangeDeclare(exchange, ExchangeType.Direct, true, false);
        channel.QueueDeclare(VideoCreatedQueue, true, false, false);
        channel.QueueBind(VideoCreatedQueue, exchange, VideoCreatedRoutingKey);

        channel.QueueDeclare(RabbitMQConfiguration.VideoEncodedQueue, true, false, false);
        channel.QueueBind(RabbitMQConfiguration.VideoEncodedQueue, exchange, VideoEncodedRoutingKey);

    }
    public void TearDownRabbitQA()
    {
        var channel = RabbitMQChannel!;
        var exchange = RabbitMQConfiguration.Exchange;

        channel.QueueUnbind(VideoCreatedQueue, exchange, VideoCreatedRoutingKey);
        channel.QueueDelete(VideoCreatedQueue, false, false);


        channel.QueueUnbind(RabbitMQConfiguration.VideoEncodedQueue, exchange, VideoEncodedRoutingKey);
        channel.QueueDelete(RabbitMQConfiguration.VideoEncodedQueue, false, false);

        channel.ExchangeDelete(exchange, false);
    }

    public override ValueTask DisposeAsync()
    {
        TearDownRabbitQA();
        return base.DisposeAsync();

    }
}


