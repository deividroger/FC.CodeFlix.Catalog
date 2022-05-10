using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Api.Configurations;

public static class ConnectionsConfiguration
{
    public static IServiceCollection AddAppConnections(this IServiceCollection services)
    {
        services.AddDbConnection();

        return services;
    }
    public static IServiceCollection AddDbConnection(this IServiceCollection services)
    {
        services.AddDbContext<CodeFlixCatalogDbContext>(options 
            => options.UseInMemoryDatabase("InMemory-DSV-Database"));
        return services;
    }
}
