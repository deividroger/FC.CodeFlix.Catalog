using FC.CodeFlix.Catalog.Api.ApiModels.CastMember;
using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.UpdateCastMember;

[Collection(nameof(CastMemberApiBaseFixture))]
public class UpdateCastMemberApiTest
{
    private readonly CastMemberApiBaseFixture _fixture;

    public UpdateCastMemberApiTest(CastMemberApiBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Update))]
    [Trait("EndToEnd/API", "CastMember/Update - Endpoints")]
    public async Task Update()
    {
        var examples = _fixture.GetExampleCastMemberList(5);
        var example = examples[2];
        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();

        await _fixture.Persistence.InsertList(examples);

        var (response, output) = await _fixture
                            .ApiClient
                            .Put<ApiResponse<CastMemberModelOutput>>($"castMembers/{example.Id}",
                                new UpdateCastMemberApiInput(newName,newType));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Data.Id.Should().Be(example.Id);
        output.Data.Name.Should().Be(newName);
        output.Data.Type.Should().Be(newType);

        var castMemberFromDb = await _fixture.Persistence.GetById(example.Id);

        castMemberFromDb.Should().NotBeNull();
        castMemberFromDb!.Name.Should().Be(newName);
        castMemberFromDb.Type.Should().Be(newType);
        castMemberFromDb.Id.Should().Be(example.Id);
    }

    [Fact(DisplayName = nameof(Return404IfNotFound))]
    [Trait("EndToEnd/API", "CastMember/Update - Endpoints")]
    public async Task Return404IfNotFound()
    {
        var examples = _fixture.GetExampleCastMemberList(5);
        var randomGuid = Guid.NewGuid();
        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();

        await _fixture.Persistence.InsertList(examples);

        var (response, output) = await _fixture
                            .ApiClient
                            .Put<ProblemDetails>($"castMembers/{randomGuid}",
                                new UpdateCastMemberApiInput(newName, newType));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);

        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found");
        output.Detail.Should().Be($"Cast Member '{randomGuid} not found.'");

    }

    [Fact(DisplayName = nameof(Return422IfThereAreValidationErrors))]
    [Trait("EndToEnd/API", "CastMember/Update - Endpoints")]
    public async Task Return422IfThereAreValidationErrors()
    {
        var examples = _fixture.GetExampleCastMemberList(5);
        var example = examples[2];
        var newName = "";
        var newType = _fixture.GetRandomCastMemberType();

        await _fixture.Persistence.InsertList(examples);

        var (response, output) = await _fixture
                            .ApiClient
                            .Put<ProblemDetails>($"castMembers/{example.Id}",
                                new UpdateCastMemberApiInput(newName, newType));

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status422UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Title.Should().Be("One or more validation errors ocurred");
        output!.Detail.Should().Be("Name should not be empty or null");

    }
}