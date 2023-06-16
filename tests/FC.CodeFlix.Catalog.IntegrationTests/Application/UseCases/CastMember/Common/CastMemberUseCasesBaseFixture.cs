using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.IntegrationTests.Base;
using System.Collections.Generic;
using DomainEntity= FC.CodeFlix.Catalog.Domain.Entity;
using System;
using System.Linq;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using Xunit;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

[CollectionDefinition(nameof(CastMemberUseCasesBaseFixture))]
public class CastMemberUseCasesBaseFixtureCollection: ICollectionFixture<CastMemberUseCasesBaseFixture> { }

public class CastMemberUseCasesBaseFixture: BaseFixture
{
    public DomainEntity.CastMember GetExampleCastMember()
        => new(GetValidName(), GetRandomCastMemberType());

    public string GetValidName()
        => Faker.Name.FullName();


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
