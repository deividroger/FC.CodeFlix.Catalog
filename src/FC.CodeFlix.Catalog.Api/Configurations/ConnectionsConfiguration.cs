using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Api.Configurations;

public static class ConnectionsConfiguration
{
    public static IServiceCollection AddAppConnections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbConnection(configuration);

        return services;
    }
    public static IServiceCollection AddDbConnection(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("catalogDb");

        services.AddDbContext<CodeFlixCatalogDbContext>(options 
            => options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
                ));
        return services;
    }
}
