
using FC.CodeFlix.Catalog.Application.UseCases.Category.CreateCategory;
using FC.CodeFlix.Catalog.EndToEndTests.Api.Category.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryApiTestFixture))]
public class CreateCategoryApiTestFixtureCollection
    : ICollectionFixture<CreateCategoryApiTestFixture>
{

}

public class CreateCategoryApiTestFixture: CategoryBaseFixture
{
    public CreateCategoryInput GetExampleInput()
        => new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

}