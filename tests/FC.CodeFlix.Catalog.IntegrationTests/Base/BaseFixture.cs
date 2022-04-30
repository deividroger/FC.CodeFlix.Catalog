using Bogus;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.IntegrationTests.Base;

public class BaseFixture
{
    public BaseFixture()
        => Faker = new("en");

    protected Faker Faker { get; set; }

    public CodeFlixCatalogDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new CodeFlixCatalogDbContext(
                new DbContextOptionsBuilder<CodeFlixCatalogDbContext>()
                    .UseInMemoryDatabase("integration-tests-db")
                    .Options
            );

        if (!preserveData)
            context.Database.EnsureDeleted();

        return context;
    }
}
