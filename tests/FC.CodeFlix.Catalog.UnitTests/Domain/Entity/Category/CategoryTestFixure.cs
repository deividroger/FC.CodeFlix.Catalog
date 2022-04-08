using FC.CodeFlix.Catalog.UnitTests.Common;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.UnitTests.Domain.Entity.Category;

public class CategoryTestFixure : BaseFixture
{
    public CategoryTestFixure():base()
    {

    }

    public string GetValidCategoryName()
    {
        var categoryName = "";

        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];

        if(categoryName.Length> 255)
            categoryName = categoryName[..255];
        
        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();

        
        if(categoryDescription.Length> 10_000)
            categoryDescription = categoryDescription[..10_000];
                
        
        return categoryDescription;
    }

    public DomainEntity.Category GetValidCategory()
        => new(GetValidCategoryName(), GetValidCategoryDescription());
}

[CollectionDefinition(nameof(CategoryTestFixure))]
public class CategoryTestFixtureCollection : ICollectionFixture<CategoryTestFixure>
{ }