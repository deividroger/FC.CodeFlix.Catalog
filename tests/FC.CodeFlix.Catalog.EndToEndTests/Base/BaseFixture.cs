using Bogus;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using Keycloak.AuthServices.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace FC.CodeFlix.Catalog.EndToEndTests.Base;

public class BaseFixture : IDisposable
{
    private readonly string _dbConnectionString;

    public BaseFixture()
    {
        Faker = new("en");

        WebAppFactory = new CustomWebApplicationFactory<Program>();

        var configuration = WebAppFactory.Services.GetRequiredService<IConfiguration>();

        var keyCloakOptions = configuration.GetSection(KeycloakAuthenticationOptions.Section)
                                           .Get<KeycloakAuthenticationOptions>();

        HttpClient = WebAppFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient, keyCloakOptions);


        ArgumentNullException.ThrowIfNull(configuration);

        _dbConnectionString = configuration.GetConnectionString("CatalogDb");
    }

    protected Faker Faker { get; set; }

    public CustomWebApplicationFactory<Program> WebAppFactory { get; set; }

    public HttpClient HttpClient { get; set; }

    public ApiClient ApiClient { get; set; }

    public CodeFlixCatalogDbContext CreateDbContext()
    {
        var context = new CodeFlixCatalogDbContext(
                new DbContextOptionsBuilder<CodeFlixCatalogDbContext>()
                    .UseMySql(
                        _dbConnectionString,
                        ServerVersion.AutoDetect(_dbConnectionString)
                    ).Options
            );

        return context;
    }

    public void CleanPersistence()
    {
        var context = CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        WebAppFactory.Dispose();
    }
}