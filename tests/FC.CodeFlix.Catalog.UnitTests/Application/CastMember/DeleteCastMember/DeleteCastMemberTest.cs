using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.DeleteCastMember;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.DeleteCastMember;

[Collection(nameof(DeleteCastMemberFixture))]
public class DeleteCastMemberTest
{
    private readonly DeleteCastMemberFixture _fixture;

    public DeleteCastMemberTest(DeleteCastMemberFixture deleteCastMemberFixture)
        => _fixture = deleteCastMemberFixture;
    
    [Fact(DisplayName = nameof(DeleteCastMember))]
    [Trait("Application","DeleteCastMember - Use Cases")]
    public async Task DeleteCastMember()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var castMemberExample = _fixture.GetExampleCastMember();
        
        repositoryMock.Setup(x=> x.Get(It.IsAny<Guid>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(castMemberExample);

        var input = new UseCase.DeleteCastMemberInput(castMemberExample.Id);
        var useCase = new UseCase.DeleteCastMember(repositoryMock.Object, unitOfWorkMock.Object);

        var action = async () => await useCase.Handle(input, CancellationToken.None);
        
        await action.Should().NotThrowAsync();

        repositoryMock.Verify(x=> x.Get(It.Is<Guid>(y=> y == input.Id ),
                                        It.IsAny<CancellationToken>()),
            Times.Once());

        repositoryMock.Verify(x => x.Delete(It.Is<DomainEntity.CastMember>(y => y.Id == input.Id),
                                        It.IsAny<CancellationToken>()),
            Times.Once());

        unitOfWorkMock.Verify(x=>x.Commit(It.IsAny<CancellationToken>()), 
            Times.Once());
    }

    [Fact(DisplayName = nameof(DeleteCastMember))]
    [Trait("Application", "DeleteCastMember - Use Cases")]
    public async Task ThrowsWhenNotFound()
    {
        var repositoryMock = new Mock<ICastMemberRepository>();

        repositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("notFound"));

        var input = new UseCase.DeleteCastMemberInput(Guid.NewGuid());
        var useCase = new UseCase.DeleteCastMember(repositoryMock.Object,  Mock.Of<IUnitOfWork>());

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}

