using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;

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

        var action = async () => await useCase.Handle(input, CancellationToken.None);

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


    [Fact(DisplayName = nameof(UpdateGenreAddingCategoriesIds))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task UpdateGenreAddingCategoriesIds()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();

        var exampleGenre = _fixture.GetExampleGenre();
        var exampleCategoriesIdsList = _fixture.GetRandomIdsList();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);

        categoryRepositoryMock.Setup(x => x
        .GetIdsListByIds(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(exampleCategoriesIdsList);

        var input = new UseCase.UpdateGenreInput(exampleGenre.Id,
            newNameExample,
            newIsActive,
            exampleCategoriesIdsList);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             categoryRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(exampleCategoriesIdsList.Count);

        exampleCategoriesIdsList.ForEach(expectedId => output.Categories.Should().Contain(expectedId));

        genreRepositoryMock.Verify(
            x => x.Update(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = nameof(UpdateGenrerReplacingCategoriesIds))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task UpdateGenrerReplacingCategoriesIds()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleGenre = _fixture.GetExampleGenre(categoriesIds: _fixture.GetRandomIdsList());
        var exampleCategoriesIdsList = _fixture.GetRandomIdsList();
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);


        categoryRepositoryMock.Setup(x => x
       .GetIdsListByIds(
               It.IsAny<List<Guid>>(),
               It.IsAny<CancellationToken>()
           )
       ).ReturnsAsync(exampleCategoriesIdsList);


        var input = new UseCase.UpdateGenreInput(exampleGenre.Id,
            newNameExample,
            newIsActive,
            exampleCategoriesIdsList);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             categoryRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(exampleCategoriesIdsList.Count);

        exampleCategoriesIdsList.ForEach(expectedId => output.Categories.Should().Contain(expectedId));

        genreRepositoryMock.Verify(
            x => x.Update(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }


    [Fact(DisplayName = nameof(UpdateGenrerReplacingCategoriesIds))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task ThrowWhenCatgegoryNotFound()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();


        var exampleGenre = _fixture.GetExampleGenre(categoriesIds: _fixture.GetRandomIdsList());

        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        var exampleNewCategoriesIdsList = _fixture.GetRandomIdsList(10);

        var listReturnedByCategoryRepository = exampleNewCategoriesIdsList
                                                    .GetRange(0, exampleNewCategoriesIdsList.Count - 2);

        var idsNotReturnedByCategoryRepository = exampleNewCategoriesIdsList
                                                    .GetRange(exampleNewCategoriesIdsList.Count - 2, 2);

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);

        categoryRepositoryMock.Setup(x => x
        .GetIdsListByIds(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(listReturnedByCategoryRepository);


        var input = new UseCase.UpdateGenreInput(exampleGenre.Id,
            newNameExample,
            newIsActive,
            exampleNewCategoriesIdsList);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             categoryRepositoryMock.Object);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        var notFoundIdsAsString = string.Join(", ", idsNotReturnedByCategoryRepository);

        await action.Should()
                    .ThrowAsync<RelatedAggregateException>()
                    .WithMessage($"Related category Id (or ids) not found : {notFoundIdsAsString}");


    }

    [Fact(DisplayName = nameof(UpdateGenrerReplacingWithoutCategoriesIds))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task UpdateGenrerReplacingWithoutCategoriesIds()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleCategoriesIdsList = _fixture.GetRandomIdsList();

        var exampleGenre = _fixture.GetExampleGenre(categoriesIds: exampleCategoriesIdsList);

        
        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);


 
        var input = new UseCase.UpdateGenreInput(exampleGenre.Id,
            newNameExample,
            newIsActive);

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             categoryRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(newNameExample);
        output.IsActive.Should().Be(newIsActive);
        output.CreatedAt.Should().BeSameDateAs(exampleGenre.CreatedAt);
        output.Id.Should().Be(exampleGenre.Id);
        output.Categories.Should().HaveCount(exampleCategoriesIdsList.Count);

        exampleCategoriesIdsList.ForEach(expectedId => output.Categories.Should().Contain(expectedId));

        genreRepositoryMock.Verify(
            x => x.Update(
                It.Is<DomainEntity.Genre>(x => x.Id == exampleGenre.Id),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = nameof(UpdateGenreWithEmptyCategoriesIdsList))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task UpdateGenreWithEmptyCategoriesIdsList()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var exampleCategoriesIdsList = _fixture.GetRandomIdsList();

        var exampleGenre = _fixture.GetExampleGenre(categoriesIds: exampleCategoriesIdsList);


        var newNameExample = _fixture.GetValidGenreName();
        var newIsActive = !exampleGenre.IsActive;

        genreRepositoryMock.Setup(x => x.Get(It.Is<Guid>(x => x == exampleGenre.Id),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(exampleGenre);



        var input = new UseCase.UpdateGenreInput(exampleGenre.Id,
            newNameExample,
            newIsActive,
            new List<Guid>());

        var useCase = new UseCase.UpdateGenre(genreRepositoryMock.Object,
                                             unitOfWorkMock.Object,
                                             categoryRepositoryMock.Object);

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

}