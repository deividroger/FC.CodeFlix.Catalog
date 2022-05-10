using FC.CodeFlix.Catalog.EndToEndTests.Base;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryBaseFixture : BaseFixture
{

    public CategoryPersistence Persistence;

    public CategoryBaseFixture() : base()
        => Persistence = new CategoryPersistence(CreateDbContext());

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

    public bool GetRandomBoolean()
        => new Random().NextDouble() < 0.5;

    public string GetInvalidNameTooShort()
        => Faker.Commerce.ProductName()[..2];

    public string GetInvalidNameTooLong()
        => Faker.Lorem.Letter(256);

    public string GetInvalidDescriptionTooLong()
        => Faker.Lorem.Letter(10_001);

    public List<DomainEntity.Category> GetExampleCategoriesList(int listLength = 15)
        => Enumerable.Range(1,listLength)
                     .Select(_ => GetExampleCategory())
                     .ToList();

    public DomainEntity.Category GetExampleCategory()
        => new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

}
