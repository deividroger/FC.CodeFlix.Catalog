using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Enum;

using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using System;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.CreateCastMember;

[Collection(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTest
{
    private readonly CreateCastMemberTestFixture _fixture;

    public CreateCastMemberTest(CreateCastMemberTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Create))]
    [Trait("Application", "CreateCastMember - UseCases")]
    public async Task Create()
    {
        var input = new UseCase.CreateCastMemberInput("Jorge Lucas", CastMemberType.Director);

        var repositoryMock = new Mock<ICastMemberRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var useCase = new UseCase.CreateCastMember(repositoryMock.Object, unitOfWorkMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);

        output.CreatedAt.Should().NotBeSameDateAs(default);

        unitOfWorkMock.Verify(x =>
                                x.Commit(It.IsAny<CancellationToken>()),
                             Times.Once);

        repositoryMock.Verify(x => x.Insert(
             It.Is<DomainEntity.CastMember>(x => x.Name == input.Name &&
                                            x.Type == input.Type),
             It.IsAny<CancellationToken>()
            ), Times.Once);
    }
}
