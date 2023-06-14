using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Repository = FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CastMemberRepository;

[Collection(nameof(CastMemberRepositoryTestFixture))]
public class CastMemberRepositoryTest
{
    private readonly CastMemberRepositoryTestFixture _fixture;

    public CastMemberRepositoryTest(CastMemberRepositoryTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Insert()
    {
        var castMemberExample = _fixture.GetExampleCastMember();
        var context = _fixture.CreateDbContext();
        var repository = new Repository.CastMemberRepository(context);

        await repository.Insert(castMemberExample, CancellationToken.None);
        await context.SaveChangesAsync();

        var assertionContext = _fixture.CreateDbContext(true);
        var castMemberFromDb = await assertionContext.CastMembers
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.Id == castMemberExample.Id);
        castMemberFromDb.Should().NotBeNull();
        castMemberFromDb!.Name.Should().Be(castMemberExample.Name);
        castMemberExample.Type.Should().Be(castMemberExample.Type);
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Get()
    {
        var castMemberExampleList = _fixture.GetExampleCastMemberList(5);
        var castMemberExample = castMemberExampleList[3];
        var arrangeContext = _fixture.CreateDbContext();
        await arrangeContext.AddRangeAsync(castMemberExampleList);
        await arrangeContext.SaveChangesAsync();


        var repository = new Repository.CastMemberRepository(_fixture.CreateDbContext(true));

        var itemFromRepository = await repository.Get(castMemberExample.Id, CancellationToken.None);

        itemFromRepository.Should().NotBeNull();
        itemFromRepository!.Name.Should().Be(castMemberExample.Name);
        itemFromRepository.Type.Should().Be(castMemberExample.Type);
    }

    [Fact(DisplayName = nameof(GetThrownWhenNotFound))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task GetThrownWhenNotFound()
    {
        var randomGuid = Guid.NewGuid();

        var repository = new Repository.CastMemberRepository(_fixture.CreateDbContext(false));

        var action = async () => await repository.Get(randomGuid, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Cast Member '{randomGuid} not found.'");
    }


    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Delete()
    {
        var castMemberExampleList = _fixture.GetExampleCastMemberList(5);
        var castMemberExample = castMemberExampleList[3];
        var arrangeContext = _fixture.CreateDbContext();
        await arrangeContext.AddRangeAsync(castMemberExampleList);
        await arrangeContext.SaveChangesAsync();
        var actDbContext = _fixture.CreateDbContext(true);

        var repository = new Repository.CastMemberRepository(actDbContext);

        await repository.Delete(castMemberExample, CancellationToken.None);
        await actDbContext.SaveChangesAsync();
        var assertionContext = _fixture.CreateDbContext(true);

        var itemInDatabase = await assertionContext.CastMembers
                                            .AsNoTracking()
                                            .ToListAsync();

        itemInDatabase.Should().HaveCount(4);
        itemInDatabase.Should().NotContain(castMemberExampleList);
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Update()
    {
        var castMemberExampleList = _fixture.GetExampleCastMemberList(5);
        var castMemberExample = castMemberExampleList[3];

        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();

        var arrangeContext = _fixture.CreateDbContext();
        await arrangeContext.AddRangeAsync(castMemberExampleList);
        await arrangeContext.SaveChangesAsync();
        var actDbContext = _fixture.CreateDbContext(true);

        var repository = new Repository.CastMemberRepository(actDbContext);

        castMemberExample.Update(newName, newType);

        await repository.Update(castMemberExample, CancellationToken.None); 
        await actDbContext.SaveChangesAsync();
        var assertionContext = _fixture.CreateDbContext(true);

        var castMemberDb = await assertionContext.CastMembers.FirstOrDefaultAsync(x=> x.Id == castMemberExample.Id);  

        castMemberDb.Should().NotBeNull();
        castMemberDb!.Name.Should().Be(newName);
        castMemberDb.Type.Should().Be(newType);
        
    }

    [Fact(DisplayName = nameof(Search))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Search()
    {
        var exampleList = _fixture.GetExampleCastMemberList(10);
        var arrangeDbContext = _fixture.CreateDbContext();
        
        await arrangeDbContext.AddRangeAsync(exampleList);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new Repository.CastMemberRepository(_fixture.CreateDbContext(true));

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var searchResult = await castMemberRepository.Search(searchInput,CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(exampleList.Count);
        searchResult.Total.Should().Be(exampleList.Count);
        searchResult.CurrentPage.Should().Be(1);
        searchResult.PerPage.Should().Be(20);

        searchResult.Items.ToList().ForEach(resultItem =>
        {
            var example = exampleList.Find(x=> x.Id == resultItem.Id); 
            example.Should().NotBeNull();

            resultItem.Name.Should().Be(resultItem.Name);
            resultItem.Type.Should().Be(resultItem.Type);

        });
    }

    [Fact(DisplayName =nameof(SearchReturnsEmptyWhenEmpty))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenEmpty()
    {
        
        var castMemberRepository = new Repository.CastMemberRepository(_fixture.CreateDbContext(false));

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var searchResult = await castMemberRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(0);
        searchResult.Total.Should().Be(0);
        searchResult.CurrentPage.Should().Be(1);
        searchResult.PerPage.Should().Be(20);

  
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]

    public async Task SearchReturnsPaginated(int quantitycastMembersToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var exampleList = _fixture.GetExampleCastMemberList(quantitycastMembersToGenerate);
        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleList);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new Repository.CastMemberRepository(_fixture.CreateDbContext(true));

        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.ASC);

        var searchResult = await castMemberRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(expectedQuantityItems);
        searchResult.Total.Should().Be(quantitycastMembersToGenerate);
        searchResult.CurrentPage.Should().Be(page);
        searchResult.PerPage.Should().Be(perPage);

        searchResult.Items.ToList().ForEach(resultItem =>
        {
            var example = exampleList.Find(x => x.Id == resultItem.Id);
            example.Should().NotBeNull();

            resultItem.Name.Should().Be(resultItem.Name);
            resultItem.Type.Should().Be(resultItem.Type);

        });
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    [InlineData("action", 1, 5, 1, 1)]
    [InlineData("horror", 1, 5, 3, 3)]
    [InlineData("horror", 2, 5, 0, 3)]
    [InlineData("sci-fi", 1, 5, 4, 4)]
    [InlineData("sci-fi", 1, 2, 2, 4)]
    [InlineData("sci-fi", 2, 3, 1, 4)]
    [InlineData("sci-fi other", 1, 3, 0, 0)]
    [InlineData("robots", 1, 5, 2, 2)]
    public async Task SearchByText(string search,
                                              int page,
                                              int perPage,
                                              int expectedQuantityItemsReturned,
                                              int expectedQuantityTotalItems)
    {
        var namesToGenerate =new List<string>() {
           "action",
           "horror",
           "horror - robots",
           "horror - bases on real facts",
           "drama",
           "sci-fi IA",
           "sci-fi Space",
           "sci-fi robots",
           "sci-fi future",
        };

        var exampleList = _fixture.GetExampleCastMemberListByNames(namesToGenerate);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleList);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new Repository.CastMemberRepository(_fixture.CreateDbContext(true));

        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.ASC);

        var searchResult = await castMemberRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(expectedQuantityItemsReturned);
        searchResult.Total.Should().Be(expectedQuantityTotalItems);
        searchResult.CurrentPage.Should().Be(page);
        searchResult.PerPage.Should().Be(perPage);

        searchResult.Items.ToList().ForEach(resultItem =>
        {
            var example = exampleList.Find(x => x.Id == resultItem.Id);
            example.Should().NotBeNull();

            resultItem.Name.Should().Be(resultItem.Name);
            resultItem.Type.Should().Be(resultItem.Type);
        });
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task SearchOrdered(string orderBy, string order)
    {
        var exampleList = _fixture.GetExampleCastMemberList(5);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.AddRangeAsync(exampleList);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new Repository.CastMemberRepository(_fixture.CreateDbContext(true));

        var searchOrder = order.ToLower() == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var searchInput = new SearchInput(1, 10, "", orderBy, searchOrder);

        var searchResult = await castMemberRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(5);
        searchResult.Total.Should().Be(5);
        searchResult.CurrentPage.Should().Be(1);
        searchResult.PerPage.Should().Be(10);

        var orderedList = _fixture.CloneCastMemberListOrdered(exampleList, orderBy, searchOrder);

        for (var i = 0; i < orderedList.Count; i++)
        {

            searchResult.Items[i].Name.Should().Be(orderedList[i]!.Name);
            searchResult.Items[i].Id.Should().Be(orderedList[i]!.Id);
            searchResult.Items[i].Type.Should().Be(orderedList[i]!.Type);
            
        }
    }
}