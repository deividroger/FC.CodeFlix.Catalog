using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.ListCastMembers;

[Collection(nameof(CastMemberUseCasesBaseFixture))]
public class ListCastMembersTest
{
    private readonly CastMemberUseCasesBaseFixture _fixture;

    public ListCastMembersTest(CastMemberUseCasesBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName =nameof(SimplesList))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    public async Task SimplesList()
    {
        var examples = _fixture.GetExampleCastMemberList(10);
        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddRangeAsync(examples);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new CastMemberRepository(
                _fixture.CreateDbContext(true)
            );

        var useCase = new UseCase.ListCastMembers(castMemberRepository);

        var input = new UseCase.ListCastMembersInput(1, 10, "", "", SearchOrder.ASC);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Items.Should().HaveCount(examples.Count);
        output.Total.Should().Be(examples.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = examples.FirstOrDefault(example => example.Id == outputItem.Id);
            exampleItem.Should().BeEquivalentTo(outputItem);
        });
    }

    [Fact(DisplayName = nameof(Empty))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    public async Task Empty()
    {
        var castMemberRepository = new CastMemberRepository(_fixture.CreateDbContext());

        var useCase = new UseCase.ListCastMembers(castMemberRepository);

        var input = new UseCase.ListCastMembersInput(1, 10, "", "", SearchOrder.ASC);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Items.Should().HaveCount(0);
        output.Total.Should().Be(0);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
    }

    [Theory(DisplayName = nameof(Pagination))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task Pagination(int quantitycastMembersToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var examples = _fixture.GetExampleCastMemberList(quantitycastMembersToGenerate);
        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddRangeAsync(examples);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new CastMemberRepository(
                _fixture.CreateDbContext(true)
            );

        var useCase = new UseCase.ListCastMembers(castMemberRepository);

        var input = new UseCase.ListCastMembersInput(page, perPage, "", "", SearchOrder.ASC);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Items.Should().HaveCount(expectedQuantityItems);
        output.Total.Should().Be(quantitycastMembersToGenerate);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = examples.FirstOrDefault(example => example.Id == outputItem.Id);
            exampleItem.Should().BeEquivalentTo(outputItem);
        });
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
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
        var namesToGenerate = new List<string>() {
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

        var castMemberRepository = new CastMemberRepository(
                _fixture.CreateDbContext(true)
            );

        var useCase = new UseCase.ListCastMembers(castMemberRepository);

        var input = new UseCase.ListCastMembersInput(page, perPage,search, "", SearchOrder.ASC);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Items.Should().HaveCount(expectedQuantityItemsReturned);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        output.Items.ToList().ForEach(outputItem =>
        {
            var exampleItem = exampleList.FirstOrDefault(example => example.Id == outputItem.Id);
            exampleItem.Should().BeEquivalentTo(outputItem);
        });
    }

    [Theory(DisplayName = nameof(Ordering))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]
    public async Task Ordering(string orderBy, string order)
    {
        var exampleList = _fixture.GetExampleCastMemberList(5);
        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddRangeAsync(exampleList);
        await arrangeDbContext.SaveChangesAsync();

        var castMemberRepository = new CastMemberRepository(_fixture.CreateDbContext(true));

        var useCase = new UseCase.ListCastMembers(castMemberRepository);

        var searchOrder = order.ToLower() == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var input = new UseCase.ListCastMembersInput(1, 10, "", orderBy, searchOrder);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().HaveCount(exampleList.Count);
        output.Total.Should().Be(exampleList.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);

        var orderedList = _fixture.CloneCastMemberListOrdered(exampleList, orderBy, searchOrder);

        for (var i = 0; i < orderedList.Count; i++)
        {
            output.Items[i].Name.Should().Be(orderedList[i]!.Name);
            output.Items[i].Id.Should().Be(orderedList[i]!.Id);
            output.Items[i].Type.Should().Be(orderedList[i]!.Type);
        }
    }
}
