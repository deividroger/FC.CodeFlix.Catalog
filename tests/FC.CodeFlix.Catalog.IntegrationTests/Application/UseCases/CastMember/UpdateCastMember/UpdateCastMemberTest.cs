
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.UpdateCastMember;

[Collection(nameof(CastMemberUseCasesBaseFixture))]
public class UpdateCastMemberTest
{
    private readonly CastMemberUseCasesBaseFixture _fixture;

    public UpdateCastMemberTest(CastMemberUseCasesBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Application", "UpdateCastMember - Use Cases")]
    public async Task Update()
    {
        var examples = _fixture.GetExampleCastMemberList(10);
        var example = examples[5];


        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(examples);
        await arrangeDbContext.SaveChangesAsync();

        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();

        var actDbContext = _fixture.CreateDbContext(true);
        var castMemberRepository = new CastMemberRepository(actDbContext);
        var unitOfWork = new UnitOfWork(actDbContext);

        var useCase = new UseCase.UpdateCastMember(castMemberRepository, unitOfWork);

        var input = new UseCase.UpdateCastMemberInput(example.Id, newName, newType);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newName);
        output.Type.Should().Be(newType);

        var item = _fixture
                        .CreateDbContext(true)
                        .CastMembers
                        .Find(example.Id);

        item.Should().NotBeNull();
        item!.Name.Should().Be(newName);
        item.Type.Should().Be(newType);

    }

    [Fact(DisplayName = nameof(ThrownWhenNotFound))]
    [Trait("Integration/Application", "UpdateCastMember - Use Cases")]
    public async Task ThrownWhenNotFound()
    {
        var examples = _fixture.GetExampleCastMemberList(10);
        var example = examples[5];
        var randomGuid = Guid.NewGuid();

        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(examples);
        await arrangeDbContext.SaveChangesAsync();

        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();

        var actDbContext = _fixture.CreateDbContext(true);
        var castMemberRepository = new CastMemberRepository(actDbContext);
        var unitOfWork = new UnitOfWork(actDbContext);

        var useCase = new UseCase.UpdateCastMember(castMemberRepository, unitOfWork);

        var input = new UseCase.UpdateCastMemberInput(randomGuid, newName, newType);

        var act = async () =>  await useCase.Handle(input, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Cast Member '{input.Id} not found.'");

    }
}
