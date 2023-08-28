
using FC.CodeFlix.Catalog.Domain.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Configurations;


internal class MediaConfiguration
    : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {

        builder.Property(media => media.Id).ValueGeneratedNever();

    }
}
