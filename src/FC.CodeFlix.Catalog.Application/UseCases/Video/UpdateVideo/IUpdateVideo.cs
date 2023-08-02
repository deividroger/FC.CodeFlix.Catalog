using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;

public interface IUpdateVideo: IRequestHandler<UpdateVideoInput, VideoModelOutput>
{
}
