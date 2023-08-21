using FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;
using FC.CodeFlix.Catalog.Domain.Extensions;

namespace FC.CodeFlix.Catalog.Api.ApiModels.Video;

public class CreateVideoApiInput
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int YearLaunched { get; set; }
    public bool Published { get; set; }
    public bool Opened { get; set; }
    public int Duration { get; set; }
    public string? Rating { get; set; }
    public List<Guid>? CategoriesIds { get; set; }
    public List<Guid>? GenresIds { get; set; }
    public List<Guid>? CastMembersIds { get; set; }

    public CreateVideoInput ToCreateVideoInput()
        => new(
                Title,
                Description,
                YearLaunched,
                Published,
                Opened,
                Duration,
                Rating.ToRating(),
                CategoriesIds,
                GenresIds,
                CastMembersIds);
}
