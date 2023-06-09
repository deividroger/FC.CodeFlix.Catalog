
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.UpdateCastMember;

[Collection(nameof(UpdateCastMemberTestFixture))]
public class UpdateCastMemberTest
{
    private readonly UpdateCastMemberTestFixture _fixture;

    public UpdateCastMemberTest(UpdateCastMemberTestFixture fixture)
     => _fixture = fixture;

    [Fact(DisplayName = nameof(Update))]
    [Trait("Application","UpdateCastMember - UseCases")]
    public async Task Update()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var castMemberExample = _fixture.GetExampleCastMember();
        var newName = _fixture.GetValidName();
        var newType = _fixture.GetRandomCastMemberType();
        repositoryMock.Setup(
                        x => x.Get(It.Is<Guid>(y=> y == castMemberExample.Id), It.IsAny<CancellationToken>())
                ).ReturnsAsync(castMemberExample);

        var input = new UseCase.UpdateCastMemberInput(castMemberExample.Id,newName,newType );

        var useCase = new UseCase.UpdateCastMember(repositoryMock.Object, unitOfWorkMock.Object);

        var ouput = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(x => x.Get(It.Is<Guid>(y => y == input.Id), It.IsAny<CancellationToken>()), 
            Times.Once);

        repositoryMock.Verify(x => x.Update(It.Is<DomainEntity.CastMember>(
            y => y.Id == input.Id &&
            y.Name == input.Name &&
            y.Type == input.Type
                        ), It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(x=> x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);

        ouput.Id.Should().Be(castMemberExample.Id);
        ouput.Name.Should().Be(newName);
        ouput.Type.Should().Be(newType);
    }

    [Fact(DisplayName = nameof(ThrownWhenNotFound))]
    [Trait("Application", "UpdateCastMember - UseCases")]
    public async Task ThrownWhenNotFound()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        repositoryMock.Setup(
                        x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
                ).ThrowsAsync(new NotFoundException("notFound"));

        var input = new UseCase.UpdateCastMemberInput(Guid.NewGuid(), _fixture.GetValidName(),_fixture.GetRandomCastMemberType());

        var useCase = new UseCase.UpdateCastMember(repositoryMock.Object, unitOfWorkMock.Object);

         var action = async  ()  => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();

    }

    [Fact(DisplayName = nameof(ThrownWhenInvalidName))]
    [Trait("Application", "UpdateCastMember - UseCases")]
    public async Task ThrownWhenInvalidName()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var castMemberExample = _fixture.GetExampleCastMember();

        repositoryMock.Setup(
                        x => x.Get(It.Is<Guid>(x=>x == castMemberExample.Id), It.IsAny<CancellationToken>())
                ).ReturnsAsync(castMemberExample);

        var input = new UseCase.UpdateCastMemberInput(castMemberExample.Id, null!, _fixture.GetRandomCastMemberType());

        var useCase = new UseCase.UpdateCastMember(repositoryMock.Object, unitOfWorkMock.Object);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>()
            .WithMessage("Name should not be empty or null");

    }
}