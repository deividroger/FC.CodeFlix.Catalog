using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UploadMedias;

public record UploadMediasInput(
    Guid VideoId,
    FileInput? VideoFile = null,
    FileInput? TrailerFile = null,
    FileInput? BannerFile = null,
    FileInput? ThumbFile = null,
    FileInput? ThumbHalfFile = null) : IRequest;