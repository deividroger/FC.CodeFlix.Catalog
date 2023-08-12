using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace FC.CodeFlix.Catalog.Infra.Data.EF;

public class UnitOfWork : IUnitOfWork
{
    private readonly CodeFlixCatalogDbContext _dbContext;
    private readonly IDomainEventPublisher _publisher;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(CodeFlixCatalogDbContext dbContext, 
                      IDomainEventPublisher publisher,
                      ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public Task Commit(CancellationToken cancellationToken)
    {
        var aggregateRoots = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.Events.Any())
            .Select(entry => entry.Entity);
            
        _logger.LogInformation("Commit: {AggregateCount} aggregate roots with events.",aggregateRoots.Count());

        var events = aggregateRoots.SelectMany(entry => entry.Events)
            .ToList();

        _logger.LogInformation("Commit: {EventsCounts} events raised.", events.Count);

        events.ForEach(async @event => 
            await _publisher.PublishAsync((dynamic) @event, cancellationToken));


        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearEvents();
        }
        
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken)
        => Task.CompletedTask;
}