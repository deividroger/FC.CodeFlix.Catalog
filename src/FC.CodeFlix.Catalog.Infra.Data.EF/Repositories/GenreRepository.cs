using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

public class GenreRepository : IGenreRepository
{

    private readonly CodeFlixCatalogDbContext _context;
    private DbSet<Genre> _genres => _context.Set<Genre>();
    private DbSet<GenresCategories> _genresCategories => _context.Set<GenresCategories>();

    public GenreRepository(CodeFlixCatalogDbContext context)
        => _context = context;

    public async Task Insert(Genre genre, CancellationToken cancellationToken)
    {
        await _genres.AddAsync(genre, cancellationToken);

        if (genre.Categories.Count > 0)
        {
            var relations = genre.Categories
                                .Select(categoryId => new GenresCategories(categoryId, genre.Id));

            await _genresCategories.AddRangeAsync(relations, cancellationToken);
        }
    }

    public async Task<Genre> Get(Guid id, CancellationToken cancellationToken)
    {
        var genre = await _genres.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        NotFoundException.ThrowIfNull(genre, $"Genre '{id}' not found.");

        var categoryIds = await _genresCategories
                            .Where(x => x.GenreId == genre.Id)
                            .Select(x => x.CategoryId)
                            .ToListAsync(cancellationToken);

        categoryIds.ForEach(genre.AddCategory);

        return genre;

    }

    public async Task Delete(Genre aggregate, CancellationToken cancellationToken)
    {
        _genresCategories.RemoveRange(
            await _genresCategories.Where(x => x.GenreId == aggregate.Id).ToListAsync(cancellationToken)
        );

        _genres.Remove(aggregate);


    }
    public async Task Update(Genre genre, CancellationToken cancellationToken)
    {
        _genres.Update(genre);

        _genresCategories.RemoveRange(await _genresCategories.Where(x => x.GenreId == genre.Id).ToListAsync());

        if (genre.Categories.Count > 0)
        {
            var relations = genre.Categories
                                .Select(categoryId => new GenresCategories(categoryId, genre.Id));

            await _genresCategories.AddRangeAsync(relations, cancellationToken);
        }

    }
    public async Task<SearchOutput<Genre>> Search(SearchInput input, CancellationToken cancellationToken)
    {
        var toSkip = (input.Page - 1) * input.PerPage;

        var query = _genres.AsNoTracking();

        query = AddOrderToQuery(query, input.OrderBy, input.Order);

        if (!string.IsNullOrWhiteSpace(input.Search))
            query = query.Where(x => x.Name.Contains(input.Search));

        var total = await query.CountAsync(cancellationToken);

        var genres = await query
                                .Skip(toSkip)
                                .Take(input.PerPage)
                                .ToListAsync(cancellationToken);

        genres.ForEach(async genre =>
        {

            (await _genresCategories
                        .Where(relations => relations.GenreId == genre.Id)
                        .Select(y => y.CategoryId)
                        .ToListAsync(cancellationToken))
                        .ForEach(genre.AddCategory);
        });


        return new SearchOutput<Genre>(input.Page, input.PerPage, total, genres);
    }

    private IQueryable<Genre> AddOrderToQuery(IQueryable<Genre> query,
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
        => await _genres.AsNoTracking()
                        .Where(genreId => ids.Contains(genreId.Id))
                        .Select(genreId => genreId.Id)
                        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Genre>> GetListByIds(List<Guid> ids, CancellationToken cancellationToken)
        => await _genres.AsNoTracking()
                        .Where(genreId => ids.Contains(genreId.Id))
                        .ToListAsync(cancellationToken);
}
