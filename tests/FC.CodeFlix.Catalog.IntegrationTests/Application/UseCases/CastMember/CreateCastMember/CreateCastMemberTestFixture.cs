﻿using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.CreateCastMember;

[CollectionDefinition(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTestFixtureCollection: ICollectionFixture<CreateCastMemberTestFixture> { }

public class CreateCastMemberTestFixture 
    : CastMemberUseCasesBaseFixture
{
}
