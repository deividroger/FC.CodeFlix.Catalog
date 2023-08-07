using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

public class CastMemberRepository : ICastMemberRepository
{
    private readonly CodeFlixCatalogDbContext _context;
    private DbSet<CastMember> _castMembers => _context.Set<CastMember>();
    public CastMemberRepository(CodeFlixCatalogDbContext context)
        => _context = context;

    public async Task Insert(CastMember aggregate, CancellationToken cancellationToken)
        => await _castMembers.AddAsync(aggregate, cancellationToken);

    public Task Delete(CastMember aggregate, CancellationToken _)
    {
        return Task.FromResult(_castMembers.Remove(aggregate));
    }

    public async Task<CastMember> Get(Guid id, CancellationToken cancellationToken)
    {
        var castMember = await  _castMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
       
        NotFoundException.ThrowIfNull(castMember, $"Cast Member '{id} not found.'");

        return castMember!;
    }

    public async Task<SearchOutput<CastMember>> Search(SearchInput input, CancellationToken cancellationToken)
    {
        var toSkip = (input.Page - 1) * input.PerPage;
        var query = _castMembers.AsNoTracking();

        query = AddOrderToQuery(query, input.OrderBy, input.Order);

        if (!string.IsNullOrEmpty(input.Search))
        {
            query = query.Where(x=> x.Name.Contains(input.Search));
        }

        var items = await query
            .Skip(toSkip)
            .Take(input.PerPage)
            .ToListAsync(cancellationToken);
        var count = await query.CountAsync(cancellationToken);

        return new SearchOutput<CastMember>(input.Page,input.PerPage, count, items);
    }

    private static IQueryable<CastMember> AddOrderToQuery(IQueryable<CastMember> query,
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

    public Task Update(CastMember aggregate, CancellationToken _)
    {
        return Task.FromResult(_castMembers.Update(aggregate));
    }

    public async Task<IReadOnlyList<Guid>> GetIdsListByIds(List<Guid> ids, CancellationToken cancellationToken)
      => await _castMembers.AsNoTracking()
                        .Where(castMemberId => ids.Contains(castMemberId.Id))
                        .Select(castMemberId => castMemberId.Id)
                        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CastMember>> GetListByIds(List<Guid> ids, CancellationToken cancellationToken)
         => await _castMembers.AsNoTracking()
                        .Where(castMemberId => ids.Contains(castMemberId.Id))
                        .ToListAsync(cancellationToken);
}