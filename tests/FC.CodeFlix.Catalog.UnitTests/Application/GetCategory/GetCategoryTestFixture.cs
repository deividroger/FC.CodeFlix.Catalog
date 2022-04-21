using FC.CodeFlix.Catalog.UnitTests.Application.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.GetCategory;


[CollectionDefinition(nameof(GetCategoryTestFixture))]
public class GetCategoryTestFixtureFixtureCollection : ICollectionFixture<GetCategoryTestFixture> { };

public class GetCategoryTestFixture : CategoryUseCasesBaseFixture
{

}
