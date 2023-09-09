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
    public List<Guid>? CategoriesId { get; set; }
    public List<Guid>? GenresId { get; set; }
    public List<Guid>? CastMembersId { get; set; }

    public CreateVideoInput ToCreateVideoInput()
        => new(
                Title,
                Description,
                YearLaunched,
                Published,
                Opened,
                Duration,
                Rating.ToRating(),
                CategoriesId,
                GenresId,
                CastMembersId);
}
