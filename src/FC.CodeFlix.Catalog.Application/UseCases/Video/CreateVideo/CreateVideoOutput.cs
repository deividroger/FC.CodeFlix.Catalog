using FC.CodeFlix.Catalog.Domain.Enum;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;

public record CreateVideoOutput(
    Guid Id,
    DateTime CreatedAt,
    string Title,
    bool Published,
    string Description,
    Rating Rating,
    int YearLaunched,
    int Duration,
    bool Opened,
    IReadOnlyCollection<Guid>? CategoriesIds = null,
    IReadOnlyCollection<Guid>? GenresIds = null,
    IReadOnlyCollection<Guid>? CastMembersIds = null,
    string? Thumb = null,
    string?Banner = null,
    string? ThumbHalf = null,
    string? Media = null,
    string? Trailer = null
    )
{
    public static CreateVideoOutput FromVideo(DomainEntity.Video video)
        => new(
            video.Id,
            video.CreatedAt,
            video.Title,
            video.Published,
            video.Description,
            video.Rating,
            video.YearLaunched,
            video.Duration,
            video.Opened,
            video.Categories,
            video.Genres,
            video.CastMembers,
            video.Thumb?.Path,
            video.Banner?.Path,
            video.ThumbHalf?.Path,
            video.Media?.FilePath,
            video.Trailer?.FilePath);
}
