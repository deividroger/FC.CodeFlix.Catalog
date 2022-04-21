﻿using FC.CodeFlix.Catalog.UnitTests.Application.Category.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Category.GetCategory;


[CollectionDefinition(nameof(GetCategoryTestFixture))]
public class GetCategoryTestFixtureFixtureCollection : ICollectionFixture<GetCategoryTestFixture> { };

public class GetCategoryTestFixture : CategoryUseCasesBaseFixture
{

}
