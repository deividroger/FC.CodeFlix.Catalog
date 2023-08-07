using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Configurations;


internal class VideosGenresConfiguration : IEntityTypeConfiguration<VideosGenres>
{
    public void Configure(EntityTypeBuilder<VideosGenres> builder)
    => builder.HasKey(relation => new
    {
        relation.GenreId,
        relation.VideoId
    });
}