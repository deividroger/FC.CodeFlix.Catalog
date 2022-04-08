using Bogus;

namespace FC.CodeFlix.Catalog.UnitTests.Common;

public class BaseFixture
{
    protected BaseFixture() => Faker = new Faker("en");

    public Faker Faker { get; set; }
}
