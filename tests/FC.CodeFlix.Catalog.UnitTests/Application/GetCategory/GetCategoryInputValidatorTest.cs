using FC.CodeFlix.Catalog.Application.UseCases.Category.GetCategory;
using FluentAssertions;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryInputValidatorTest
{
    private readonly GetCategoryTestFixture _getCategoryTestFixture;

    public GetCategoryInputValidatorTest(GetCategoryTestFixture getCategoryTestFixture)
        => _getCategoryTestFixture = getCategoryTestFixture;
    
    [Fact(DisplayName = nameof(ValidationOk))]
    [Trait("Application","GetCategoryInputValidation - UseCases")]
    public void ValidationOk()
    {
        var validInput = new GetCategoryInput(Guid.NewGuid());

        var validator = new GetCategoryInputValidator();

       var validationResult =  validator.Validate(validInput);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().HaveCount(0);

    }

    [Fact(DisplayName = nameof(InvalidationWhenEmptyId))]
    [Trait("Application", "GetCategoryInputValidation - UseCases")]
    public void InvalidationWhenEmptyId()
    {
        var invalidInput = new GetCategoryInput(Guid.Empty);

        var validator = new GetCategoryInputValidator();
            
        var validationResult = validator.Validate(invalidInput);

        validationResult.Should().NotBeNull();  
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors[0].ErrorMessage.Should().Be($"'Id' must not be empty.");
        
    }

}
