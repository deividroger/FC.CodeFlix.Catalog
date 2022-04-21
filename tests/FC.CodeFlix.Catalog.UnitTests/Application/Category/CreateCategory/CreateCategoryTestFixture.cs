using FC.CodeFlix.Catalog.Application.UseCases.Category.CreateCategory;
using FC.CodeFlix.Catalog.UnitTests.Application.Category.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTestFixtureCollection : ICollectionFixture<CreateCategoryTestFixture>
{

}
public class CreateCategoryTestFixture : CategoryUseCasesBaseFixture
{
    public CreateCategoryInput GetInput()
        => new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

    public CreateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetInput();
        invalidInputShortName.Name = invalidInputShortName.Name.Substring(0, 2);
        return invalidInputShortName;

    }

    public CreateCategoryInput GetInvalidInputTooLongName()
    {

        var invalidInputTooLongName = GetInput();
        invalidInputTooLongName.Name = Faker.Lorem.Letter(256);

        return invalidInputTooLongName;
    }

    public CreateCategoryInput GetInvalidInputDescriptionNull()
    {
        var invalidInputDescriptionNull = GetInput();
        invalidInputDescriptionNull.Description = null!;

        return invalidInputDescriptionNull;
    }

    public CreateCategoryInput GetInvalidInputTooLongDescription()
    {

        var invalidInputTooLongDescription = GetInput();
        invalidInputTooLongDescription.Description = Faker.Lorem.Letter(10_001);

        return invalidInputTooLongDescription;
    }


}
