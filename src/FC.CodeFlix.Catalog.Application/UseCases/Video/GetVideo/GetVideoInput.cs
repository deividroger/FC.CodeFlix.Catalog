
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.GetVideo;

public record GetVideoInput(Guid videoId): IRequest<VideoModelOutput>;
