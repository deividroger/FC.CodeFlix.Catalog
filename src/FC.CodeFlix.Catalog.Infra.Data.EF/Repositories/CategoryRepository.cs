
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CodeFlixCatalogDbContext _context;
    private DbSet<Category> _categories => _context.Set<Category>();

    public CategoryRepository(CodeFlixCatalogDbContext context)
    {
        _context = context;
    }

    public async Task Insert(Category aggregate, CancellationToken cancellationToken)
    {
        await _categories.AddAsync(aggregate, cancellationToken);
    }

    public Task Delete(Category aggregate, CancellationToken _)
    {
        return Task.FromResult(_categories.Remove(aggregate));
    }

    public async Task<Category> Get(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        NotFoundException.ThrowIfNull(category, $"category '{id}' not found.");

        return category!;
    }

    public async Task<SearchOutput<Category>> Search(SearchInput input, CancellationToken cancellationToken)
    {

        var toSkip = (input.Page - 1) * input.PerPage;
        var query = _categories.AsNoTracking();

        query = AddOrderToQuery(query, input.OrderBy, input.Order);

        if (!string.IsNullOrWhiteSpace(input.Search))
            query = query.Where(x => x.Name.Contains(input.Search));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
                            .Skip(toSkip)
                            .Take(input.PerPage).ToListAsync(cancellationToken);

        return new SearchOutput<Category>(input.Page, input.PerPage, total, items);
    }

    public Task Update(Category aggregate, CancellationToken _)
    {
        return Task.FromResult(_categories.Update(aggregate));
    }

    private static IQueryable<Category> AddOrderToQuery(IQueryable<Category> query,
                                                string orderProperty,
                                                SearchOrder order)
     => (orderProperty.ToLower(), order) switch
     {
         ("name", SearchOrder.ASC) => query.OrderBy(x => x.Name)
            .ThenBy(x => x.Id),
         ("name", SearchOrder.DESC) => query.OrderByDescending(x => x.Name)
            .ThenByDescending(x => x.Id),
         ("id", SearchOrder.ASC) => query.OrderBy(x => x.Id),
         ("id", SearchOrder.DESC) => query.OrderByDescending(x => x.Id),

         ("createdat", SearchOrder.ASC) => query.OrderBy(x => x.CreatedAt),
         ("createdat", SearchOrder.DESC) => query.OrderByDescending(x => x.CreatedAt),
         _ => query.OrderBy(x => x.Name)
                   .ThenBy(x => x.Id)
     };

    public async Task<IReadOnlyList<Guid>> GetIdsListByIds(List<Guid> ids, CancellationToken cancellationToken)
        => await _categories.AsNoTracking()
                      .Where(category => ids.Contains(category.Id))
                      .Select(category => category.Id)
                      .ToListAsync(cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Category>> GetListByIds(List<Guid> ids, CancellationToken cancellationToken)
        => await _categories.AsNoTracking()
                          .Where(category => ids.Contains(category.Id))
                          .ToListAsync(cancellationToken: cancellationToken);
}
