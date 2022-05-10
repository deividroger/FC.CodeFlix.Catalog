using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.UpdateCategory
{
    public class UpdateCategoryApiTestDataGenerator
    {
        public static IEnumerable<object[]> GetInvalidInputs()
        {

            var fixture = new UpdateCategoryApiTestFixture();

            var invalidInputList = new List<object[]>();

            var totalInvalidCases = 3;

            for (int index = 0; index < totalInvalidCases; index++)
            {
                switch (index % totalInvalidCases)
                {
                    case 0:
                        var input1 = fixture.GetExampleInput();
                        input1.Name = fixture.GetInvalidNameTooShort();
                        invalidInputList.Add(new object[] { input1, "Name should be at least 3 characters long" });
                        break;
                    case 1:
                        var input2 = fixture.GetExampleInput();
                        input2.Name = fixture.GetInvalidNameTooLong();
                        invalidInputList.Add(new object[] { input2, "Name should be less or equal 255 characters long" });
                        break;
                    case 2:
                        var input3 = fixture.GetExampleInput();
                        input3.Description = fixture.GetInvalidDescriptionTooLong();
                        invalidInputList.Add(new object[] { input3, "Description should be less or equal 10000 characters long" });
                        break;
                    default:
                        break;
                }
            }

            return invalidInputList;
        }
    }
}
