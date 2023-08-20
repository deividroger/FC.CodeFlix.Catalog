using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Application.EventHandlers;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Category.CreateCategory;
using FC.CodeFlix.Catalog.Domain.Events;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FC.CodeFlix.Catalog.Infra.Messaging.Configuration;
using FC.CodeFlix.Catalog.Infra.Messaging.Producer;
using FC.CodeFlix.Catalog.Infra.Storage.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FC.CodeFlix.Catalog.Api.Configurations;

public static class UseCasesConfiguration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddMediatR(typeof(CreateCategory));
        services.AddRepositories();
        services.AddDomainEvents(configuration);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ICategoryRepository, CategoryRepository>();

        services.AddTransient<IGenreRepository, GenreRepository>();

        services.AddTransient<ICastMemberRepository, CastMemberRepository>();

        services.AddTransient<IVideoRepository, VideoRepository>();

        services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddDomainEvents(this IServiceCollection services, IConfiguration configuration)
    {
         services.AddTransient<IDomainEventPublisher, DomainEventPublisher>();

        services.AddTransient<IDomainEventHandler<VideoUploadedEvent>, SendToEncoderEventHandler>();


        services.Configure<RabbitMQConfiguration>(
            configuration.GetSection(RabbitMQConfiguration.ConfigurationSection));

        services.AddSingleton(sp =>  { 
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

        services.AddTransient<IMessageProducer>(sp =>
        {
            var channelManager = sp.GetRequiredService<ChannelManager>();
            var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>();

            return new RabbitMQProducer(channelManager.GetChannel(), config);

        });

        return services;
    }
}