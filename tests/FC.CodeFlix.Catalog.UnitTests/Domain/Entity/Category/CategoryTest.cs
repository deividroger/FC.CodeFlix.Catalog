using FC.CodeFlix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Lw.Reflection.CompilerServices;
using System;
using System.Collections.Generic;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.UnitTests.Domain.Entity.Category;

[Collection(nameof(CategoryTestFixure))]
public class CategoryTest
{
    private readonly CategoryTestFixure _categoryTestFixure;

    public CategoryTest(CategoryTestFixure categoryTestFixure)
        => _categoryTestFixure = categoryTestFixure;

    [Fact(DisplayName = nameof(Instanciate))]
    [Trait("Domain","Category - Agregates")]
    public void Instanciate()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var datetimeBefore = DateTime.Now;

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        var datetimeAfter = DateTime.Now.AddMilliseconds(10);

        category.Should().NotBeNull();

        category.Name.Should().Be(validCategory.Name);
        
        category.Description.Should().Be(validCategory.Description);

        category.Id.Should().NotBeEmpty();


        category.CreatedAt.Should().NotBeSameDateAs(default(DateTime));

        (category.CreatedAt >= datetimeBefore).Should().BeTrue();

        (category.CreatedAt <= datetimeAfter).Should().BeTrue();

        category.IsActive.Should().BeTrue();


    }


    [Theory(DisplayName = nameof(InstanciateWithIsActive))]
    [Trait("Domain", "Category - Agregates")]
    [InlineData(true)]
    [InlineData(false)]
    public void InstanciateWithIsActive(bool isActive)
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var datetimeBefore = DateTime.Now;

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description,isActive);

        var datetimeAfter = DateTime.Now.AddMilliseconds(10);

        category.Should().NotBeNull();


        category.Name.Should().Be(validCategory.Name);
        category.Description.Should().Be(validCategory.Description);

        category.Id.Should().NotBeEmpty();



        category.CreatedAt.Should().NotBeSameDateAs(default(DateTime));

        (category.CreatedAt >= datetimeBefore).Should().BeTrue();

        (category.CreatedAt <= datetimeAfter).Should().BeTrue();

        (category.IsActive).Should().Be(isActive);

    }

    [Theory(DisplayName = nameof(InstanciateErrorWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void InstanciateErrorWhenNameIsEmpty(string? name)
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        Action action = 
            () => new DomainEntity.Category(name!, validCategory.Description);

        action.Should().Throw<EntityValidationException>().WithMessage("Name should not be empty or null");

    }

    [Fact(DisplayName = nameof(InstanciateErrorWhenDescriptionIsNull))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstanciateErrorWhenDescriptionIsNull()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        Action action =
            () => new DomainEntity.Category(validCategory.Name, null!);

        action.Should().Throw<EntityValidationException>().WithMessage("Description should not be null");

    }

    [Theory(DisplayName = nameof(InstantiateErrorWhenNameIsLessThan3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [MemberData(nameof(GetNamesWithLessThan3Characters),parameters:10)]
    public void InstantiateErrorWhenNameIsLessThan3Characters(string invalidName)
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        Action action =
             () => new DomainEntity.Category(invalidName, validCategory.Description);

        action.Should().Throw<EntityValidationException>().WithMessage("Name should be at least 3 characters long");

    }

    [Fact(DisplayName = nameof(InstantiateErrorWhenNameIsGreaterThen250Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateErrorWhenNameIsGreaterThen250Characters()
    {
        var invalidName = _categoryTestFixure.Faker.Lorem.Letter(256);

        var validCategory = _categoryTestFixure.GetValidCategory();

        Action action =
             () => new DomainEntity.Category(invalidName, validCategory.Description);


        action.Should().Throw<EntityValidationException>().WithMessage("Name should be less or equal 255 characters long");

    }

    [Fact(DisplayName = nameof(InstantiateErrorWhenDescriptionIsGreaterThen10_000Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateErrorWhenDescriptionIsGreaterThen10_000Characters()
    {
        var invalidDescription = _categoryTestFixure.Faker.Lorem.Letter(10_001);

        var validCategory = _categoryTestFixure.GetValidCategory();

        Action action = () => new DomainEntity.Category(validCategory.Name, invalidDescription);

        action.Should().Throw<EntityValidationException>().WithMessage("Description should be less or equal 10000 characters long");
        
    }

    [Fact(DisplayName = nameof(Activate))]
    [Trait("Domain", "Category - Agregates")]    
    public void Activate()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description, false);

        category.Active();
        
        category.IsActive.Should().BeTrue();

    }

    [Fact(DisplayName = nameof(Deativate))]
    [Trait("Domain", "Category - Agregates")]
    public void Deativate()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description, true);

        category.Deativate();

        category.IsActive.Should().BeFalse();

        Assert.False(category.IsActive);

    }


    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Category - Agregates")]
    public void Update()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);
        var categoryWithnewValues = _categoryTestFixure.GetValidCategory();

        category.Update(categoryWithnewValues.Name, categoryWithnewValues.Description);


        category.Name.Should().Be(categoryWithnewValues.Name);
        category.Description.Should().Be(categoryWithnewValues.Description);

    }

    [Fact(DisplayName = nameof(UpdateOnlyName))]
    [Trait("Domain", "Category - Agregates")]
    public void UpdateOnlyName()
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);
        var newName = _categoryTestFixure.GetValidCategoryName();
        var currentDescription = category.Description;

        category.Update(newName);

        category.Name.Should().Be(newName);

        category.Description.Should().Be(currentDescription); 
        
    }


    [Theory(DisplayName = nameof(UpdateErrorWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void UpdateErrorWhenNameIsEmpty(string? name)
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        Action action =
            () => category.Update(name!);

        action.Should().Throw<EntityValidationException>().WithMessage("Name should not be empty or null"); 

    }

    [Theory(DisplayName = nameof(UpdateErrorWhenNameIsLessThan3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [MemberData(nameof(GetNamesWithLessThan3Characters), parameters: 10)]
    public void UpdateErrorWhenNameIsLessThan3Characters(string invalidName)
    {
        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        Action action =
             () => category.Update(invalidName);


        action.Should().Throw<EntityValidationException>().WithMessage("Name should be at least 3 characters long");

    }

    [Fact(DisplayName = nameof(UpdateErrorWhenNameIsGreaterThen255Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateErrorWhenNameIsGreaterThen255Characters()
    {
        var invalidName = _categoryTestFixure.Faker.Lorem.Letter(256);

        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        Action action =
             () => category.Update(invalidName);

        action.Should().Throw<EntityValidationException>().WithMessage("Name should be less or equal 255 characters long");

    }


    [Fact(DisplayName = nameof(UpdateErrorWhenDescriptionIsGreaterThen10_000Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateErrorWhenDescriptionIsGreaterThen10_000Characters()
    {
        var invalidDescription = _categoryTestFixure.Faker.Lorem.Random.String(10_001);

        var validCategory = _categoryTestFixure.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        Action action = () => category.Update("category Name", invalidDescription);

        var exception = Assert.Throws<EntityValidationException>(action);

        action.Should().Throw<EntityValidationException>().WithMessage("Description should be less or equal 10000 characters long");

    }

    public static IEnumerable<object[]> GetNamesWithLessThan3Characters(int numerOfTests)
    {
        var fixture = new CategoryTestFixure();

        for (int i = 0; i < numerOfTests; i++)
        {
            var isOdd = i % 2 == 1;

             yield return new object[] { fixture.GetValidCategoryName()[..(isOdd ? 1 : 2)] };
        }
    }
}