using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;

using FluentAssertions;
using Moq;
using System;
using System.Threading;

using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Exceptions;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest
{
    private readonly UpdateGenreTestFixture _fixture;

    public UpdateGenreTest(UpdateGenreTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task UpdateGenre()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);

        var input = new UseCase.UpdateGenreInput(exampleGenre.Id, newNameExample, newIsActive);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             _fixture.GetCategoryRepositoryMock().Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(0);

        genreRepositoryMock.Verify(
            x => x.Update(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = nameof(ThrowWhenNotFound))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task ThrowWhenNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();

        var exampleId = Guid.NewGuid();

        genreRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>(),
                                             It.IsAny<CancellationToken>())
        ).ThrowsAsync(new NotFoundException($"Genre '{exampleId}' not found."));

        var input = new UseCase.UpdateGenreInput(exampleId, _fixture.GetValidGenreName(), true);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             _fixture.GeUnitOfWorkMock().Object,
                                             _fixture.GetCategoryRepositoryMock().Object);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{exampleId}' not found.");

    }

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Application", "CreateGenre - Use Cases")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task ThrowWhenNameIsInvalid(string? name)
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);

        var input = new UseCase.UpdateGenreInput(exampleGenre.Id, name!, newIsActive);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             _fixture.GetCategoryRepositoryMock().Object);

        var action = async ()=> await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>().WithMessage($"Name should not be empty or null");

    }


    [Theory(DisplayName = nameof(UpdateGenreOnlyName))]
    [Trait("Application", "CreateGenre - Use Cases")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateGenreOnlyName(bool isActive)
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre(isActive: isActive);
        var newNameExample = _fixture.GetValidGenreName();


        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);

        var input = new UseCase.UpdateGenreInput(exampleGenre.Id, newNameExample);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             _fixture.GetCategoryRepositoryMock().Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(isActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(0);

        genreRepositoryMock.Verify(
            x => x.Update(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }
}