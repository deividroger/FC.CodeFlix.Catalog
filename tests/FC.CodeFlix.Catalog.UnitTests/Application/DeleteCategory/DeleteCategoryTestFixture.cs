using FC.CodeFlix.Catalog.UnitTests.Application.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.DeleteCategory;

[CollectionDefinition(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTestFixtureCollection : ICollectionFixture<DeleteCategoryTestFixture>
{
}

public class DeleteCategoryTestFixture : CategoryUseCasesBaseFixture
{

}
