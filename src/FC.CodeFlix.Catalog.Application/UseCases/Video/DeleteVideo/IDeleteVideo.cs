using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.DeleteVideo;

public interface IDeleteVideo: IRequestHandler<DeleteVideoInput>
{
}
