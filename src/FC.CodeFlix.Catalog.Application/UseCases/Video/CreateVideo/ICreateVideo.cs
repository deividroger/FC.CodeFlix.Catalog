using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;

public interface ICreateVideo: IRequestHandler<CreateVideoInput, VideoModelOutput>
{
}
