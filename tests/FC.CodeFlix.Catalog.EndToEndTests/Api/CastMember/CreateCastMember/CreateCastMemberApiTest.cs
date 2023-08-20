using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;
using FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.CreateCastMember;

[Collection(nameof(CastMemberApiBaseFixture))]
public class CreateCastMemberApiTest: IDisposable
{
    private readonly CastMemberApiBaseFixture _fixture;

    public CreateCastMemberApiTest(CastMemberApiBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Create))]
    [Trait("EndToEnd/API", "CastMember/Create - Endpoints")]
    public async Task Create()
    {
        var example = _fixture.GetExampleCastMember();

        var (response, output) = await _fixture
                                           .ApiClient
                                           .Post<TestApiResponse<CastMemberModelOutput>>("castMembers",
                                                new CreateCastMemberInput(example.Name, example.Type));
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);

        output.Should().NotBeNull();
        output!.Data!.Id.Should().NotBeEmpty();
        output.Data!.Name.Should().Be(example.Name);
        output.Data!.Type.Should().Be(example.Type);

        var castMemberInDb = await _fixture.Persistence.GetById(output.Data.Id);

        castMemberInDb.Should().NotBeNull();
        castMemberInDb!.Name.Should().Be(example.Name);
        castMemberInDb.Type.Should().Be(example.Type);
    }

    [Fact(DisplayName = nameof(ThrownWhenNameIsEmpty))]
    [Trait("EndToEnd/API", "CastMember/Create - Endpoints")]
    public async Task ThrownWhenNameIsEmpty()
    {
        var example = _fixture.GetExampleCastMember();

        var (response, output) = await _fixture
                                           .ApiClient
                                           .Post<ProblemDetails>("castMembers",
                                                new CreateCastMemberInput(string.Empty, example.Type));
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status422UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Title.Should().Be("One or more validation errors ocurred");
        output!.Detail.Should().Be("Name should not be empty or null");
    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }
}
