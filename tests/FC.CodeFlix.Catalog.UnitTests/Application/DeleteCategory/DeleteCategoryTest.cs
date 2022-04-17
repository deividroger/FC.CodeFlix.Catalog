using FC.CodeFlix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UsesCase = FC.CodeFlix.Catalog.Application.UseCases.Category.DeleteCategory;

namespace FC.CodeFlix.Catalog.UnitTests.Application.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;

    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
       => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Application", "Delete category - Use Cases")]
    public async Task DeleteCategory()
    {
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var categoryExample = _fixture.GetValidCategory();

        repositoryMock.Setup(x => x.Get(categoryExample.Id, It.IsAny<CancellationToken>()
                )).ReturnsAsync(categoryExample);

        var input = new UsesCase.DeleteCategoryInput(categoryExample.Id);
        var useCase = new UsesCase.DeleteCategory(repositoryMock.Object, unitOfWork.Object);

        await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(x => x.Get(categoryExample.Id, It.IsAny<CancellationToken>()), Times.Once);

        repositoryMock.Verify(x => x.Delete(categoryExample, It.IsAny<CancellationToken>()), Times.Once);

        unitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

    }


    [Fact(DisplayName = nameof(ThrownWhenCategoryNotFound))]
    [Trait("Application", "Delete category - Use Cases")]
    public async Task ThrownWhenCategoryNotFound()
    {
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();
        var exampleGuid = Guid.NewGuid();

        repositoryMock.Setup(x => x.Get(exampleGuid, It.IsAny<CancellationToken>()
                )).ThrowsAsync(new NotFoundException($"Category '{exampleGuid}' not found"));

        var input = new UsesCase.DeleteCategoryInput(exampleGuid);
        var useCase = new UsesCase.DeleteCategory(repositoryMock.Object, unitOfWork.Object);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>();
        
        repositoryMock.Verify(x => x.Get(exampleGuid, It.IsAny<CancellationToken>()), Times.Once);

    }
}
