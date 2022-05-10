using Bogus;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FC.CodeFlix.Catalog.EndToEndTests.Base;

public class BaseFixture
{
    public BaseFixture()
    {
        Faker = new("en");
        WebAppFactory = new CustomWebApplicationFactory<Program>();
        HttpClient = WebAppFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
    }
        

    protected Faker Faker { get; set; }

    public CustomWebApplicationFactory<Program> WebAppFactory { get; set; }

    public HttpClient HttpClient { get; set; }

    public ApiClient ApiClient { get; set; }

    public CodeFlixCatalogDbContext CreateDbContext()
    {
        var context = new CodeFlixCatalogDbContext(
                new DbContextOptionsBuilder<CodeFlixCatalogDbContext>()
                    .UseInMemoryDatabase("end2end-test-db")
                    .Options
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