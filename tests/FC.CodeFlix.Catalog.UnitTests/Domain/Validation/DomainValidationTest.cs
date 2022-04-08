using Bogus;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Validation;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.Validation;

public class DomainValidationTest
{
    private Faker Faker { get; set; } = new Faker();

    [Fact(DisplayName = nameof(NotNullOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullOk()
    {
        var value = Faker.Commerce.ProductName();

        Action action = () => DomainValidation.NotNull(value, "value");

        action.Should().NotThrow();

    }

    [Fact(DisplayName = nameof(NotNullThrowWhenNull))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullThrowWhenNull()
    {
        string? value = null;
        
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.NotNull(value, fieldName);

        action.Should().Throw<EntityValidationException>().WithMessage($"{fieldName} should not be null");

    }

    [Theory(DisplayName = nameof(NotNullOrEmpyThrowWhenEmpy))]
    [Trait("Domain", "DomainValidation - Validation")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NotNullOrEmpyThrowWhenEmpy(string? target)
    {
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.NotNullOrEmpty(target, fieldName);

        action.Should().Throw<EntityValidationException>().WithMessage($"{fieldName} should not be empty or null");
    }

    [Fact(DisplayName = nameof(NotNullOrEmpy))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullOrEmpy()
    {
        var target = Faker.Commerce.ProductName();
        
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.NotNullOrEmpty(target, fieldName);

        action.Should().NotThrow();
    }

    [Theory(DisplayName = nameof(MinLengthThrowWhenLess))]
    [Trait("Domain", "DomainValidation - Validation")]
    [MemberData(nameof(GetValuesSmallerThanMin), parameters: 10)]
    public void MinLengthThrowWhenLess(string target, int minLength)
    {
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.MinLength(target, minLength, fieldName);

        action.Should().Throw<EntityValidationException>().WithMessage($"{fieldName} should be at least {minLength} characters long");

    }

    [Theory(DisplayName = nameof(MinLengthOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    [MemberData(nameof(GetValuesSmallerGreaterMin), parameters: 10)]
    public void MinLengthOk(string target, int minLength)
    {
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.MinLength(target, minLength, fieldName);

        action.Should().NotThrow();

    }

    public static IEnumerable<object[]> GetValuesSmallerThanMin(int numberOfTestes = 5)
    {
        yield return new object[] { "12346", 10 };

        var faker = new Faker();

        for (int i = 0; i < (numberOfTestes - 1); i++)
        {
            var example = faker.Commerce.ProductName();

            var minLength = example.Length + (new Random().Next(1, 20));

            yield return new object[] { example, minLength };
        }
    }

    public static IEnumerable<object[]> GetValuesSmallerGreaterMin(int numberOfTestes = 5)
    {
        

        var faker = new Faker();

        for (int i = 0; i < (numberOfTestes ); i++)
        {
            var example = faker.Commerce.ProductName();

            var minLength = example.Length - (new Random().Next(1, 5));

            yield return new object[] { example, minLength };
        }
    }
        
    [Theory(DisplayName =nameof(MaxLengthThrowWhenGreater))]
    [Trait("Domain", "DomainValidation - Validation")]
    [MemberData(nameof(GetValuesGreaterMax), parameters: 10)]
    public void MaxLengthThrowWhenGreater(string target, int maxLength)
    {
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action  action =  () =>  DomainValidation.MaxLength(target, maxLength, fieldName);

        action.Should().Throw<EntityValidationException>().WithMessage($"{fieldName} should be less or equal {maxLength} characters long");

    }

    [Theory(DisplayName = nameof(MaxLengthOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    [MemberData(nameof(GetValuesLessThanMax), parameters: 10)]
    public void MaxLengthOk(string target, int maxLength)
    {
        var fieldName = Faker.Commerce.ProductName().Replace(" ", "");

        Action action = () => DomainValidation.MaxLength(target, maxLength, fieldName);

        action.Should().NotThrow();

    }

    public static IEnumerable<object[]> GetValuesGreaterMax(int numberOfTestes = 5)
    {

        var faker = new Faker();

        for (int i = 0; i < (numberOfTestes); i++)
        {
            var example = faker.Commerce.ProductName();

            var maxLength = example.Length - (new Random().Next(1, 5));

            yield return new object[] { example, maxLength };
        }
    }

    public static IEnumerable<object[]> GetValuesLessThanMax(int numberOfTestes = 5)
    {
        var faker = new Faker();

        for (int i = 0; i < numberOfTestes; i++)
        {
            var example = faker.Commerce.ProductName();

            var maxLength = example.Length + (new Random().Next(1, 5));

            yield return new object[] { example, maxLength };
        }
    }

}