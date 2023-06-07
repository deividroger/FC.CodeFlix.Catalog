using FC.CodeFlix.Catalog.UnitTests.Application.CastMember.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.GetCastMember;


[CollectionDefinition(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTestFixtureCollection: ICollectionFixture<GetCastMemberTestFixture> { }

public class GetCastMemberTestFixture: CastMemberUseCasesBaseFixture
{

}
