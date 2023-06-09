using FC.CodeFlix.Catalog.UnitTests.Application.CastMember.Common;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.ListCastMembers;


[CollectionDefinition(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTestFixtureCollection: ICollectionFixture<ListCastMembersTestFixture>
{ }
public class ListCastMembersTestFixture : CastMemberUseCasesBaseFixture
{
    public List<DomainEntity.CastMember> GetExampleCastMemberList(int quantity)
        => Enumerable.Range(1,quantity)
        .Select(_ => GetExampleCastMember())
        .ToList();
}
