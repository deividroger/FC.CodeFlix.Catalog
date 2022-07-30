using FC.CodeFlix.Catalog.UnitTests.Application.CastMember.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.CreateCastMember;

[CollectionDefinition(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTestFixtureCollection: ICollectionFixture<CreateCastMemberTestFixture> { }

public class CreateCastMemberTestFixture
    : CastMemberUseCasesBaseFixture
{
}
