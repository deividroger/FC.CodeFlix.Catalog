using FC.CodeFlix.Catalog.Application.UseCases.Category.ListCategories;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.UnitTests.Application.Category.Common;
using System;
using System.Collections.Generic;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTestFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>
{

}

public class ListCategoriesTestFixture : CategoryUseCasesBaseFixture
{


    public List<DomainEntity.Category> GetExampleCategoriesList(int length = 10)
    {
        var list = new List<DomainEntity.Category>();

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
}
