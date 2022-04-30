using System.Collections.Generic;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.CreateCategory;

public class CreateCategoryTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs(int times = 12)
    {
        var fixture = new CreateCategoryTestFixture();

        var invalidInputList = new List<object[]>();

        var totalInvalidCases = 4;

        for (int index = 0; index < times; index++)
        {
            switch (index % totalInvalidCases)
            {
                case 0:
                    invalidInputList.Add(new object[] { fixture.GetInvalidInputShortName(), "Name should be at least 3 characters long" });
                    break;
                case 1:
                    invalidInputList.Add(new object[] { fixture.GetInvalidInputTooLongName(), "Name should be less or equal 255 characters long" });
                    break;
                case 2:
                    invalidInputList.Add(new object[] { fixture.GetInvalidInputDescriptionNull(), "Description should not be null" });
                    break;
                case 3:
                    invalidInputList.Add(new object[] { fixture.GetInvalidInputTooLongDescription(), "Description should be less or equal 10000 characters long" });
                    break;
                default:
                    break;
            }
        }

        return invalidInputList;
    }



}
