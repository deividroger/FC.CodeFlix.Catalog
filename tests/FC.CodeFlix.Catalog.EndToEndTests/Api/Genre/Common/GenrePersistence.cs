using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.Common
{
    public class GenrePersistence
    {
        private readonly CodeFlixCatalogDbContext _context;

        public GenrePersistence(CodeFlixCatalogDbContext context)
            => _context = context;

        public async Task InsertList(List<DomainEntity.Genre> genres)
        {
            await _context.AddRangeAsync(genres);
            await _context.SaveChangesAsync();
        }

        public async Task InsertGenresCategoriesRelationsList(List<GenresCategories>  relations)
        {
            await _context.AddRangeAsync(relations);
            await _context.SaveChangesAsync();
        }

        public async Task<DomainEntity.Genre?> GetById(Guid id)
            => await _context.Genres
                            .AsNoTracking()
                            .FirstOrDefaultAsync(genre => genre.Id == id);

        public async Task<List<GenresCategories>> GetGenresCategoriesRelationsById(Guid id)
            => await _context.GenresCategories
                             .AsNoTracking()
                             .Where(relation => relation.GenreId == id)
                             .ToListAsync();
    }
}
