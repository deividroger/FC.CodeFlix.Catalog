using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.DeleteVideo;

public record DeleteVideoInput(Guid VideoId): IRequest;

