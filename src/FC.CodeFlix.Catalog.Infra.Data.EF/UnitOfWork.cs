using FC.CodeFlix.Catalog.Application.Interfaces;

namespace FC.CodeFlix.Catalog.Infra.Data.EF;

public class UnitOfWork : IUnitOfWork
{
    private readonly CodeFlixCatalogDbContext _dbContext;

    public UnitOfWork(CodeFlixCatalogDbContext dbContext)
        => _dbContext = dbContext;
    

    public Task Commit(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public Task Rollback(CancellationToken cancellationToken)
        => Task.CompletedTask;
}