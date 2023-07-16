using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UploadMedias;

public record UploadMediasInput(
    Guid VideoId,
    FileInput? VideoFile,
    FileInput? TrailerFile) : IRequest;