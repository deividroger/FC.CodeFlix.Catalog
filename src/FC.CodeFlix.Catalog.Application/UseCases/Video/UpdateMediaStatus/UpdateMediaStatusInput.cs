
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;

public record class UpdateMediaStatusInput
    (Guid VideoId,
    MediaStatus Status,
    string? EncodedPath = null,
    string? ErrorMessage = null) : IRequest<VideoModelOutput>;
