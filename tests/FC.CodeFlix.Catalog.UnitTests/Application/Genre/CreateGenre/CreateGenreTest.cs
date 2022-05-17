using FC.CodeFlix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCases = FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest
{
    private readonly CreateGenreTestFixture _fixture;

    public CreateGenreTest(CreateGenreTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Create))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task Create()
    {
        var datetimeBefore = DateTime.Now;

        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();

        var useCases = new UseCases.CreateGenre(genreRepositoryMock.Object, unitOfWorkMock.Object, categoryRepositoryMock.Object);

        var input = _fixture.GetExampleInput();

        var datetimeAfter = DateTime.Now.AddMilliseconds(10);


        var output = await useCases.Handle(input, CancellationToken.None);

        genreRepositoryMock.Verify(x => x.Insert(
            It.IsAny<DomainEntity.Genre>(),
            It.IsAny<CancellationToken>()
            ), Times.Once);

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Id.Should().NotBeEmpty();
        output.Categories.Should().HaveCount(0);
        output.CreatedAt.Should().NotBeSameDateAs(default(DateTime));
        (output.CreatedAt >= datetimeBefore).Should().BeTrue();
        (output.CreatedAt <= datetimeAfter).Should().BeTrue();
    }


    [Fact(DisplayName = nameof(CreateWithRelatedCategories))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateWithRelatedCategories()
    {
        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();

        var input = _fixture.GetExampleInputWithCategories();

        categoryRepositoryMock.Setup(
                                    x => x.GetIdsListByIds(It.IsAny<List<Guid>>(),
                                    It.IsAny<CancellationToken>())
                               ).ReturnsAsync(input.CategoriesIds!);


        var useCases = new UseCases.CreateGenre(genreRepositoryMock.Object, unitOfWorkMock.Object, categoryRepositoryMock.Object);

        


        var output = await useCases.Handle(input, CancellationToken.None);

        genreRepositoryMock.Verify(x => x.Insert(
            It.IsAny<DomainEntity.Genre>(),
            It.IsAny<CancellationToken>()
            ), Times.Once);

        unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Id.Should().NotBeEmpty();
        output.Categories.Should().HaveCount(input.CategoriesIds?.Count ?? 0);

        input.CategoriesIds?.ForEach(id => output.Categories.Should().Contain(id));

        output.CreatedAt.Should().NotBeSameDateAs(default(DateTime));
    }


    [Fact(DisplayName = nameof(CreateThrowWhenRelatedCategoryNotFound))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateThrowWhenRelatedCategoryNotFound()
    {

        var unitOfWorkMock = _fixture.GeUnitOfWorkMock();

        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();


        var input = _fixture.GetExampleInputWithCategories();

        var exampleGuid = input.CategoriesIds![^1];

        categoryRepositoryMock.Setup(
                                    x => x.GetIdsListByIds(It.IsAny<List<Guid>>(),
                                    It.IsAny<CancellationToken>())
                               ).ReturnsAsync(input.CategoriesIds.FindAll(x => x != exampleGuid));

        var useCases = new UseCases.CreateGenre(genreRepositoryMock.Object,
                                                unitOfWorkMock.Object,
                                                categoryRepositoryMock.Object);

        var action = async () => await useCases.Handle(input, CancellationToken.None);


        await action.Should().ThrowAsync<RelatedAggregateException>().WithMessage($"Related category Id (or ids) not found : {exampleGuid}");


        categoryRepositoryMock.Verify(x => x.GetIdsListByIds(It.IsAny<List<Guid>>(),
                                    It.IsAny<CancellationToken>()), Times.Once);

    }

}
