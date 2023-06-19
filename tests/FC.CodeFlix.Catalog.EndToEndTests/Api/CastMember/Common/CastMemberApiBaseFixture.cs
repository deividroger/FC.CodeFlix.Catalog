using Bogus;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;


[CollectionDefinition(nameof(CastMemberApiBaseFixture))]
public class CastMemberApiBaseFixtureCollection : ICollectionFixture<CastMemberApiBaseFixture>
{
}

public class CastMemberApiBaseFixture : BaseFixture
{
    public CastMemberPersistence Persistence;
    public CastMemberApiBaseFixture() : base()
    {
        Persistence = new CastMemberPersistence(
             CreateDbContext()
            );
    }

    public string GetValidName()
     => Faker.Name.FullName();

    public DomainEntity.CastMember GetExampleCastMember()
        => new(GetValidName(), GetRandomCastMemberType());

    public CastMemberType GetRandomCastMemberType()
        => (CastMemberType)new Random().Next(1, 2);

    public List<DomainEntity.CastMember> GetExampleCastMemberList(int quantity)
        => Enumerable.Range(1, quantity)
            .Select(_ => GetExampleCastMember())
            .ToList();

    public List<DomainEntity.CastMember> GetExampleCastMemberListByNames(List<string> names)
        => names.Select(name =>
        {
            var example = GetExampleCastMember();
            example.Update(name, example.Type);
            return example;
        }).ToList();

    public List<DomainEntity.CastMember> CloneCastMemberListOrdered(List<DomainEntity.CastMember> castMembersList, string orderBy, SearchOrder order)
    {
        var listClone = new List<DomainEntity.CastMember>(castMembersList);

        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.ASC) => listClone.OrderBy(x => x.Name)
                .ThenBy(x => x.Id),
            ("name", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Name)
                .ThenByDescending(x => x.Id),

            ("id", SearchOrder.ASC) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.DESC) => listClone.OrderByDescending(x => x.Id),

            ("createdat", SearchOrder.ASC) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.DESC) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name)
                   .ThenBy(x => x.Id),
        };

        return orderedEnumerable.ToList();

    }


}
