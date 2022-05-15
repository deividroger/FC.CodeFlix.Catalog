using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryPersistence
{

    private readonly CodeFlixCatalogDbContext _context;

    public CategoryPersistence(CodeFlixCatalogDbContext context)
        => _context = context;


    public Task<DomainEntity.Category?> GetById(Guid id)
        => _context.Categories.AsNoTracking()
                             .FirstOrDefaultAsync(c => c.Id == id);

    public async Task InsertList(List<DomainEntity.Category> categories)
    {
        await _context.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
    }
}
