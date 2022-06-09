using FC.CodeFlix.Catalog.Infra.Data.EF;
using System.Collections.Generic;
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
    }
}
