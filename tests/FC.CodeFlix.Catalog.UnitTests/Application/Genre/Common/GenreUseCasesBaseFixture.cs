using FC.CodeFlix.Catalog.UnitTests.Common;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCasesBaseFixture:BaseFixture
{
    public string GetValidGenreName()
        => Faker.Commerce.Categories(1)[0];
}
