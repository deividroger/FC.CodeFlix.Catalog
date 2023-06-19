using FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.DeleteCastMember;

[Collection(nameof(CastMemberApiBaseFixture))]
public class DeleteCastMemberApiTest
{
    private readonly CastMemberApiBaseFixture _fixture;

    public DeleteCastMemberApiTest(CastMemberApiBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Delete))]
    [Trait("EndToEnd/API", "CastMember/Delete - Endpoints")]
    public async Task Delete()
    {
        var examples = _fixture.GetExampleCastMemberList(5);
        var example = examples[2];

        await _fixture.Persistence.InsertList(examples);

        var (response, _) = await _fixture
                            .ApiClient
                            .Delete<object>($"castMembers/{example.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status204NoContent);

        var castMemberExample = await _fixture.Persistence.GetById(example.Id);
        castMemberExample.Should().BeNull();
    }

    [Fact(DisplayName = nameof(NotFound))]
    [Trait("EndToEnd/API", "CastMember/Delete - Endpoints")]
    public async Task NotFound()
    {
        var randomGuid = Guid.NewGuid();

        var (response, output) = await _fixture
                            .ApiClient
                            .Delete<ProblemDetails>($"castMembers/{randomGuid}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);

        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found");
        output.Detail.Should().Be($"Cast Member '{randomGuid} not found.'");
    }
}