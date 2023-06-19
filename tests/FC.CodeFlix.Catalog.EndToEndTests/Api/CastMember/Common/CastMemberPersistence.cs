using FC.CodeFlix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;


namespace FC.CodeFlix.Catalog.EndToEndTests.Api.CastMember.Common;

public class CastMemberPersistence 
{
    private readonly CodeFlixCatalogDbContext _context;

    public CastMemberPersistence(CodeFlixCatalogDbContext context)
        => _context = context;

    public async Task InsertList(List<DomainEntity.CastMember> castMembers)
    {
        await _context.AddRangeAsync(castMembers);
        await _context.SaveChangesAsync();
    }

    public async Task<DomainEntity.CastMember?> GetById(Guid id)
        => await _context.CastMembers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(genre => genre.Id == id);
}
