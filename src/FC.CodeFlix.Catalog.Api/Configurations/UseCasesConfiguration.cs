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
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddMediatR(typeof(CreateCategory));
        services.AddRepositories();
        services.AddDomainEvents();

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

    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
         services.AddTransient<IDomainEventPublisher, DomainEventPublisher>();

        services.AddTransient<IDomainEventHandler<VideoUploadedEvent>, SendToEncoderEventHandler>();

        
        return services;
    }
}