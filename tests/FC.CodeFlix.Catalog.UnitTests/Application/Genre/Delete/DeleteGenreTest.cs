using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FluentAssertions;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.Delete;

[Collection(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTest
{
    private readonly DeleteGenreTestFixture _fixture;

    public DeleteGenreTest(DeleteGenreTestFixture fixture)
         => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Application", "Delete - Use Cases")]
    public async Task DeleteGenre()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre();


        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);


        var useCase = new UseCase.DeleteGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object);

        var input = new UseCase.DeleteGenreInput(exampleGenre.Id);

        await useCase.Handle(input, CancellationToken.None);

        genreRepositoryMock.Verify(
            x => x.Get(
            It.Is<Guid>(x => x == exampleGenre.Id),
            It.IsAny<CancellationToken>()
            )
        , Times.Once());


        genreRepositoryMock.Verify(
            x => x.Delete(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Application", "DeleteGenre - Use Cases")]
    public async Task ThrowWhenNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();


        var exampleId = Guid.NewGuid();
        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleId),
                                             It.IsAny<CancellationToken>())
        ).ThrowsAsync(new NotFoundException($"Genre '{exampleId}' not found."));


        var useCase = new UseCase.DeleteGenre(genreRepositoryMock.Object,
                                           _fixture.GeUnitOfWorkMock().Object);

        var input = new UseCase.DeleteGenreInput(exampleId);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{exampleId}' not found.");


        genreRepositoryMock.Verify(
            x => x.Get(
                It.Is<Guid>(x => x == exampleId),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

    }
}