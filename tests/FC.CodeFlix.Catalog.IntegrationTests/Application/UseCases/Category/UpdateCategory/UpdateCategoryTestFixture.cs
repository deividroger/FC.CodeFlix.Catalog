using FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTestFixtureCollection: ICollectionFixture<UpdateCategoryTestFixture> { }

public class UpdateCategoryTestFixture: CategoryUseCasesBaseFixture
{
    public UpdateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetValidInput();
        invalidInputShortName.Name = invalidInputShortName.Name.Substring(0, 2);
        return invalidInputShortName;

    }

    public UpdateCategoryInput GetInvalidInputTooLongName()
    {

        var invalidInputTooLongName = GetValidInput();
        invalidInputTooLongName.Name = Faker.Lorem.Letter(256);

        return invalidInputTooLongName;
    }

    public UpdateCategoryInput GetInvalidInputTooLongDescription()
    {

        var invalidInputTooLongDescription = GetValidInput();
        invalidInputTooLongDescription.Description = Faker.Lorem.Letter(10_001);

        return invalidInputTooLongDescription;
    }

    public UpdateCategoryInput GetValidInput(Guid? id = null)
     => new(id ?? Guid.NewGuid(),
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean());
}
