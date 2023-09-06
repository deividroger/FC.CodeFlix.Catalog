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

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if(environment == Environments.Development)
            return app;

        using var scope = app.Services.CreateScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<CodeFlixCatalogDbContext>();
        dbContext.Database.Migrate();
        return app;
    }
}
