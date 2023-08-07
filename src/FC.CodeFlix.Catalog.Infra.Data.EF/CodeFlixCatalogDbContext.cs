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

    public DbSet<CastMember> CastMembers => Set<CastMember>();

    public DbSet<GenresCategories> GenresCategories 
        => Set<GenresCategories>();

    public DbSet<VideosCategories> VideosCategories 
        => Set<VideosCategories>();

    public DbSet<VideosGenres> VideosGenres
        => Set<VideosGenres>();

    public DbSet<VideosCastMembers> VideosCastMembers
        => Set<VideosCastMembers>();

    public DbSet<Video> Videos
        => Set<Video>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CategoryConfiguration());
        builder.ApplyConfiguration(new GenreConfiguration());
        builder.ApplyConfiguration(new VideoConfiguration());


        builder.ApplyConfiguration(new GenresCategoriesConfiguration());
        builder.ApplyConfiguration(new VideosCategoriesConfiguration());
        builder.ApplyConfiguration(new VideosGenresConfiguration());
        builder.ApplyConfiguration(new VideosCastMembersConfiguraton());
        builder.ApplyConfiguration(new MediaConfiguration());
    }
}
