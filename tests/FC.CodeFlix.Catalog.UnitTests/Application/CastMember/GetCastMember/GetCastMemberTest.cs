using FC.CodeFlix.Catalog.Domain.Repository;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FC.CodeFlix.Catalog.Application.Exceptions;

namespace FC.CodeFlix.Catalog.UnitTests.Application.CastMember.GetCastMember;

[Collection(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTest
{
    private readonly GetCastMemberTestFixture _fixture;

    public GetCastMemberTest(GetCastMemberTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(GetCastMember))]
    [Trait("Application", "GetCastMember - Use Cases")]
    public async Task GetCastMember()
    {

        var repositoryMock = new Mock<ICastMemberRepository>();
        var castMemberExample = _fixture.GetExampleCastMember();

        repositoryMock.Setup(x =>
                             x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
                             ).ReturnsAsync(castMemberExample);

        var input = new UseCase.GetCastMemberInput(castMemberExample.Id);
        var useCase = new UseCase.GetCastMember(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(castMemberExample.Id);
        output.Name.Should().Be(castMemberExample.Name);
        output.Type.Should().Be(castMemberExample.Type);

        repositoryMock.Verify(x =>
                            x.Get(It.Is<Guid>(x => x == input.Id),
                                  It.IsAny<CancellationToken>()
                            ),
                            Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowIfNotFound))]
    [Trait("Application", "GetCastMember - Use Cases")]
    public async Task ThrowIfNotFound()
    {

        var repositoryMock = new Mock<ICastMemberRepository>();

        repositoryMock.Setup(x =>
                             x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>())
                             ).ThrowsAsync(new NotFoundException("not found"));

        var input = new UseCase.GetCastMemberInput(Guid.NewGuid());
        var useCase = new UseCase.GetCastMember(repositoryMock.Object);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
       
    }
}
