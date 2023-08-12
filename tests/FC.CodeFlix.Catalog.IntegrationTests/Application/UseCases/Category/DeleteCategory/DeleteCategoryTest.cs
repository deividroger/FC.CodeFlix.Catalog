using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using UsesCase = FC.CodeFlix.Catalog.Application.UseCases.Category.DeleteCategory;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;

    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Integration/Application", "Delete category - Use Cases")]
    public async Task DeleteCategory()
    {
        var dbContext = _fixture.CreateDbContext();

        
        var repositoryMock = new CategoryRepository(dbContext);


        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var unitOfWork = new UnitOfWork(dbContext, eventPublisher, logger);


        var categoryExample = _fixture.GetExampleCategory();

        var exampleList = _fixture.GetExampleCategoriesList();


        await dbContext.AddRangeAsync(exampleList);
        var trackingInfo = await dbContext.AddAsync(categoryExample);

        await dbContext.SaveChangesAsync();
        trackingInfo.State = EntityState.Detached;

        var input = new UsesCase.DeleteCategoryInput(categoryExample.Id);
        var useCase = new UsesCase.DeleteCategory(repositoryMock, unitOfWork);

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbCategoryDeleted = await assertDbContext.Categories.FindAsync(categoryExample.Id);

        dbCategoryDeleted.Should().BeNull();
        var dbCategories = await assertDbContext.Categories.ToListAsync();
        dbCategories.Should().HaveCount(exampleList.Count);

    }


    [Fact(DisplayName = nameof(DeleteCategoryThrowsWhenNotFound))]
    [Trait("Integration/Application", "Delete category - Use Cases")]
    public async Task DeleteCategoryThrowsWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();

        var repositoryMock = new CategoryRepository(dbContext);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var unitOfWork = new UnitOfWork(dbContext, eventPublisher, logger);

        var exampleList = _fixture.GetExampleCategoriesList();


        await dbContext.AddRangeAsync(exampleList);

        await dbContext.SaveChangesAsync();


        var input = new UsesCase.DeleteCategoryInput(Guid.NewGuid());
        var useCase = new UsesCase.DeleteCategory(repositoryMock, unitOfWork);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>().WithMessage($"category '{input.Id}' not found.");

    }
}
