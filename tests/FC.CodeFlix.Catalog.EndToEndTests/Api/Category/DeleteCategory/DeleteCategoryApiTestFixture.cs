using FC.CodeFlix.Catalog.EndToEndTests.Api.Category.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.DeleteCategory;

[CollectionDefinition(nameof(DeleteCategoryApiTestFixture))]
public class DeleteCategoryApiTestFixtureCollection: ICollectionFixture<DeleteCategoryApiTestFixture>
{

}

public class DeleteCategoryApiTestFixture
    : CategoryBaseFixture
{
}