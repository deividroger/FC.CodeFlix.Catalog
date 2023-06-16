
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.GetCastMember;

[Collection(nameof(CastMemberUseCasesBaseFixture))]
public class GetCastMemberTest
{
    private readonly CastMemberUseCasesBaseFixture _fixture;

    public GetCastMemberTest(CastMemberUseCasesBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof( GetCastMember))]
    [Trait("Integration/Application","GetCastMember - Use Cases")]
    public async Task GetCastMember()
    {
        var examples = _fixture.GetExampleCastMemberList(10);
        var example = examples[5];
        
        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddRangeAsync(examples);
        await arrangeDbContext.SaveChangesAsync();

        var useCase = new UseCase.GetCastMember(
                new CastMemberRepository(_fixture.CreateDbContext(true)
            ));
        var input = new GetCastMemberInput(example.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(example.Id);
        output.Name.Should().Be(example.Name);
        output.Type.Should().Be(example.Type);
    }

    [Fact(DisplayName = nameof(ThrownWhenNotFound))]
    [Trait("Integration/Application", "GetCastMember - Use Cases")]
    public async Task ThrownWhenNotFound()
    {
        var useCase = new UseCase.GetCastMember(
                new CastMemberRepository(_fixture.CreateDbContext(false)
            ));
        var input = new GetCastMemberInput(Guid.NewGuid());

        var act = async() =>  await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Cast Member '{input.Id} not found.'");        
    }
}
