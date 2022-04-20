using FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.UnitTests.Common;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTestFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>
{

}

public class ListCategoriesTestFixture : BaseFixture
{
    public Mock<ICategoryRepository> GetRepositoryMock() => new();

    public string GetValidCategoryName()
    {
        var categoryName = "";

        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];

        if (categoryName.Length > 255)
            categoryName = categoryName[..255];

        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();


        if (categoryDescription.Length > 10_000)
            categoryDescription = categoryDescription[..10_000];


        return categoryDescription;
    }

    public List<Category> GetExampleCategoriesList(int length = 10)
    {
        var list = new List<Category>();

        for (int i = 0; i < length; i++)
        {
            list.Add(GetExampleCategory());
        }
        return list;
    }

    public ListCategoriesInput GetExampleInput()
    {
        var random = new Random();

        return new(
                 page: random.Next(1, 10),
                 perPage: random.Next(15, 100),
                 search: Faker.Commerce.ProductName(),
                 sort: Faker.Commerce.ProductName(),
                 dir: random.Next(0, 10) > 5 ? SearchOrder.ASC : SearchOrder.DESC
             );
    }


    public bool GetRandomBoolean()
        => (new Random().NextDouble()) < 0.5;

    public Category GetExampleCategory()
        => new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());


}
