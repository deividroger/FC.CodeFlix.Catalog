using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Infra.Messaging.Configuration;
using FC.CodeFlix.Catalog.Infra.Messaging.Consumer;
using FC.CodeFlix.Catalog.Infra.Messaging.Producer;
using FC.CodeFlix.Catalog.Infra.Storage.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FC.CodeFlix.Catalog.Api.Configurations;

public static class MessagingConfiguration
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQConfiguration>(
          configuration.GetSection(RabbitMQConfiguration.ConfigurationSection));

        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;

            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
                Port = config.Port
            };

            return factory.CreateConnection();
        });

        services.AddSingleton<ChannelManager>();

        return services;
    }

    public static IServiceCollection AddMessageProducer(this IServiceCollection services)
    {
        services.AddTransient<IMessageProducer>(sp =>
        {
            var channelManager = sp.GetRequiredService<ChannelManager>();
            var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>();

            return new RabbitMQProducer(channelManager.GetChannel(), config);

        });

        return services;
    }

    public static IServiceCollection AddMessageConsumer(this IServiceCollection services)
    {
        services.AddHostedService(sp =>
        {

            var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>();
            var connection = sp.GetRequiredService<IConnection>();
            var logger = sp.GetRequiredService<ILogger<VideoEncodedEventConsumer>>();

            return new VideoEncodedEventConsumer(
                sp, logger, config, connection.CreateModel());
        });

        return services;
    }
}