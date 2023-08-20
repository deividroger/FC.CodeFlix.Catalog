using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Infra.Storage.Configuration;
using FC.CodeFlix.Catalog.Infra.Storage.Services;
using Google.Cloud.Storage.V1;

namespace FC.CodeFlix.Catalog.Api.Configurations;

public static class StorageConfiguration
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {

        
        services.Configure<StorageServiceOptions>(
            configuration.GetSection(StorageServiceOptions.ConfigurationSection)
            );

        services.AddTransient<IStorageService, StorageService>();
        services.AddScoped(_ => StorageClient.Create());

        return services;
    }
}   
