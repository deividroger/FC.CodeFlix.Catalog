
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;
using System.Linq;
using System.Collections.Generic;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.ListCastMembers;


[Collection(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTest
{
    private readonly ListCastMembersTestFixture _listCastMembersTestFixture;

    public ListCastMembersTest(ListCastMembersTestFixture listCastMembersTestFixture)
     => _listCastMembersTestFixture = listCastMembersTestFixture;

    [Fact(DisplayName = nameof(List))]
    [Trait("Application", "ListCastMembers - Use Cases")]
    public async Task List()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var castMembersListExample = _listCastMembersTestFixture.GetExampleCastMemberList(3);

        var repositorySearchOutput = new SearchOutput<DomainEntity.CastMember>(1, 10, castMembersListExample.Count, castMembersListExample);

        repositoryMock.Setup(x => x.Search(
                It.IsAny<SearchInput>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(repositorySearchOutput);


        var input = new UseCase.ListCastMembersInput(1, 10, "", "", SearchOrder.ASC);

        var useCase = new UseCase.ListCastMembers(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();

        output.Page.Should().Be(repositorySearchOutput.CurrentPage);
        output.PerPage.Should().Be(repositorySearchOutput.PerPage);
        output.Total.Should().Be(repositorySearchOutput.Total);

        output.Items.ToList().ForEach(outputItem =>
        {
            var example = castMembersListExample.Find(x => x.Id == outputItem.Id);
            example.Should().NotBeNull();
            outputItem.Name.Should().Be(example!.Name);
            outputItem.Type.Should().Be(example.Type);

        });

        repositoryMock.Verify(x => x.Search(
            It.Is<SearchInput>(y =>
                y.Page == input.Page &&
                y.PerPage == input.PerPage &&
                y.Search == input.Search &&
                y.Order == input.Dir &&
                y.OrderBy == input.Sort)
            , It.IsAny<CancellationToken>()), Times.Once());
    }


    [Fact(DisplayName = nameof(ReturnEmptyWhenIsEmpty))]
    [Trait("Application", "ListCastMembers - Use Cases")]
    public async Task ReturnEmptyWhenIsEmpty()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var castMembersListExample = new List<DomainEntity.CastMember>();

        var repositorySearchOutput = new SearchOutput<DomainEntity.CastMember>(1, 10, castMembersListExample.Count, castMembersListExample);

        repositoryMock.Setup(x => x.Search(
                It.IsAny<SearchInput>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(repositorySearchOutput);


        var input = new UseCase.ListCastMembersInput(1, 10, "", "", SearchOrder.ASC);

        var useCase = new UseCase.ListCastMembers(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();

        output.Page.Should().Be(repositorySearchOutput.CurrentPage);
        output.PerPage.Should().Be(repositorySearchOutput.PerPage);
        output.Total.Should().Be(repositorySearchOutput.Total);

        output.Items.Should().HaveCount(castMembersListExample.Count);

        repositoryMock.Verify(x => x.Search(
            It.Is<SearchInput>(y =>
                y.Page == input.Page &&
                y.PerPage == input.PerPage &&
                y.Search == input.Search &&
                y.Order == input.Dir &&
                y.OrderBy == input.Sort)
            , It.IsAny<CancellationToken>()), Times.Once());
    }
}
