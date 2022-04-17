﻿using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;

namespace FC.CodeFlix.Catalog.UnitTests.Application.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest
{
    private readonly UpdateCategoryTestFixture _fixture;

    public UpdateCategoryTest(UpdateCategoryTestFixture fixture)
        => _fixture = fixture;


    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Application", "Update category - Use Cases")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategory(Category exampleCategory, 
                                    UseCase.UpdateCategoryInput input)
    {
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();

        
        repositoryMock.Setup(x => x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(exampleCategory);

      
        var useCase = new UseCase.UpdateCategory(repositoryMock.Object, unitOfWork.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();

        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive);

        repositoryMock.Verify(x => x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()), Times.Once());

        repositoryMock.Verify(x => x.Update(exampleCategory, It.IsAny<CancellationToken>()), Times.Once);

        unitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Application", "Update category - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWork = _fixture.GetUnitOfWorkMock();

        var input = _fixture.GetValidInput();

        repositoryMock.Setup(x => x.Get(input.Id, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException($"category {input.Id} not found"));


        var useCase = new UseCase.UpdateCategory(repositoryMock.Object, unitOfWork.Object);

        
        var task = async() => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>();

        repositoryMock.Verify(x => x.Get(input.Id, It.IsAny<CancellationToken>()), Times.Once());



    }

}