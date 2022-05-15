using Bogus;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace FC.CodeFlix.Catalog.EndToEndTests.Base;

public class BaseFixture
{
    private readonly string _dbConnectionString;

    public BaseFixture()
    {
        Faker = new("en");
        WebAppFactory = new CustomWebApplicationFactory<Program>();
        HttpClient = WebAppFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
        var configuration = WebAppFactory.Services.GetService(typeof(IConfiguration));

        ArgumentNullException.ThrowIfNull(configuration);

        _dbConnectionString = ((IConfiguration)configuration).GetConnectionString("CatalogDb");
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
}