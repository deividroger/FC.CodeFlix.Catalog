using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UnitOfWorkInfra = FC.CodeFlix.Catalog.Infra.Data.EF;
namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.UnitOfWork;

[Collection(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTest
{
    private readonly UnitOfWorkTestFixture _fixture;

    public UnitOfWorkTest(UnitOfWorkTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Commit))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Persistence")]
    public async Task Commit()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoriesList = _fixture.GetExampleCategoriesList();

        var categoryWithEvents = exampleCategoriesList.First();

        var @event = new DomainEventFake();
        categoryWithEvents.RaiseEvent(@event);

        var eventHandlerMock = new Mock<IDomainEventHandler<DomainEventFake>>();

        await dbContext.AddRangeAsync(exampleCategoriesList);

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(eventHandlerMock.Object);

        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWorkInfra.UnitOfWork>>();

        var unitOfWork = new UnitOfWorkInfra.UnitOfWork(dbContext, eventPublisher, logger);

        await unitOfWork.Commit(CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);

        var savedCategories = assertDbContext.Categories
                                    .AsNoTracking()
                                    .ToList();

        savedCategories.Should().HaveCount(exampleCategoriesList.Count);

        eventHandlerMock.Verify(x => 
        x.HandleAsync(@event, It.IsAny<CancellationToken>()),
            Times.Once);

        categoryWithEvents.Events.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(Rollback))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Persistence")]
    public async Task Rollback()
    {
        var dbContext = _fixture.CreateDbContext();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWorkInfra.UnitOfWork>>();

        var unitOfWork = new UnitOfWorkInfra.UnitOfWork(dbContext, eventPublisher, logger);

        var task = async () => await unitOfWork.Rollback(CancellationToken.None);

        await task.Should().NotThrowAsync();

    }
}
