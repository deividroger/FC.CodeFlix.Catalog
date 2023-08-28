using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;

public interface IUpdateMediaStatus: 
    IRequestHandler<UpdateMediaStatusInput, VideoModelOutput>
{
}
