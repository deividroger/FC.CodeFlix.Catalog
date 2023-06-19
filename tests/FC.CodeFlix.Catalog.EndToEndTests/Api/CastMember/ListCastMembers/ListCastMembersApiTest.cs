
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.ListCastMembers;

[Collection(nameof(CastMemberApiBaseFixture))]
public class ListCastMembersApiTest : IDisposable
{
    private readonly CastMemberApiBaseFixture _fixture;

    public ListCastMembersApiTest(CastMemberApiBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(List))]
    [Trait("EndToEnd/API", "CastMember/List - Endpoints")]
    public async Task List()
    {
        var examples = _fixture.GetExampleCastMemberList(5);

        await _fixture.Persistence.InsertList(examples);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<CastMemberModelOutput>>("castMembers");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(examples.Count);

        output.Data.Should().NotBeNull();
        output.Data.Should().HaveCount(examples.Count);

        output.Data!.ToList().ForEach(outputItem =>
        {

            var exampleItem = examples.Find(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Type.Should().Be(exampleItem.Type);

        });
    }

    [Theory(DisplayName = nameof(Pagination))]
    [Trait("EndToEnd/API", "CastMember/List - Endpoints")]
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

        await _fixture.Persistence.InsertList(examples);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<CastMemberModelOutput>>("castMembers",
                                                new ListCastMembersInput(page, perPage, "", ""));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(examples.Count);

        output.Data.Should().NotBeNull();
        output.Data.Should().HaveCount(expectedQuantityItems);

        output.Data!.ToList().ForEach(outputItem =>
        {

            var exampleItem = examples.Find(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Type.Should().Be(exampleItem.Type);

        });
    }

    [Fact(DisplayName = nameof(ReturnEmptyWhenEmpty))]
    [Trait("EndToEnd/API", "CastMember/List - Endpoints")]
    public async Task ReturnEmptyWhenEmpty()
    {

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<CastMemberModelOutput>>("castMembers");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(0);

        output.Data.Should().NotBeNull();
        output.Data.Should().HaveCount(0);

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
        var castMembers = _fixture.GetExampleCastMemberListByNames(namesToGenerate);
        await _fixture.Persistence.InsertList(castMembers);

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<CastMemberModelOutput>>("castMembers",
                                                new ListCastMembersInput(page, perPage, search, ""));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(expectedQuantityTotalItems);

        output.Data.Should().NotBeNull();
        output.Data.Should().HaveCount(expectedQuantityItemsReturned);

        output.Data!.ToList().ForEach(outputItem =>
        {

            var exampleItem = castMembers.Find(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();

            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Type.Should().Be(exampleItem.Type);

        });

    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]
    public async Task Ordering(string orderBy, string order) {

        var exampleList = _fixture.GetExampleCastMemberList(5);
        await _fixture.Persistence.InsertList(exampleList);

        var searchOrder = order.ToLower() == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var (response, output) = await _fixture.ApiClient
                                               .Get<TestApiResponseList<CastMemberModelOutput>>("castMembers",
                                                new ListCastMembersInput(1, 10, "", orderBy, searchOrder));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();

        output.Meta!.Total.Should().Be(exampleList.Count);

        output.Data.Should().NotBeNull();
        output.Data.Should().HaveCount(exampleList.Count);

        var orderedList = _fixture.CloneCastMemberListOrdered(exampleList, orderBy, searchOrder);

        for (var i = 0; i < orderedList.Count; i++)
        {
            output.Data![i].Name.Should().Be(orderedList[i]!.Name);
            output.Data[i].Id.Should().Be(orderedList[i]!.Id);
            output.Data[i].Type.Should().Be(orderedList[i]!.Type);
        }

    }

    public void Dispose() 
        => _fixture.CleanPersistence();
}