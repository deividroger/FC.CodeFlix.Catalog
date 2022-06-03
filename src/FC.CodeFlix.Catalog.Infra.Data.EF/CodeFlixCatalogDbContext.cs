using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Infra.Data.EF.Configurations;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF;

public class CodeFlixCatalogDbContext :
    DbContext
{
    public CodeFlixCatalogDbContext(DbContextOptions<CodeFlixCatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories 
        => Set<Category>();

    public DbSet<Genre> Genres => 
        Set<Genre>();

    public DbSet<GenresCategories> GenresCategories 
        => Set<GenresCategories>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CategoryConfiguration());
        builder.ApplyConfiguration(new GenreConfiguration());

        builder.ApplyConfiguration(new GenresCategoriesConfiguration());
    }
}
