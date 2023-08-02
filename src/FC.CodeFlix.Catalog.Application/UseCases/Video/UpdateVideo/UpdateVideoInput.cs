using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;

public record UpdateVideoInput(
        Guid VideoId,
        string Title,
        string Description,
        int YearLaunched,
        bool Published,
        bool Opened,
        int Duration,
        Rating Rating,
        List<Guid>? GenresIds = null,
        List<Guid>? CategoriesIds = null,
        List<Guid>? CastMembersIds = null,
        FileInput? Banner = null,
        FileInput? Thumb = null,
        FileInput? ThumbHalf = null
    ) : IRequest<VideoModelOutput>;

