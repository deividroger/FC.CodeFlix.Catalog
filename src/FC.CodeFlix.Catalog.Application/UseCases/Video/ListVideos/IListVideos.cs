
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos
{
    public interface IListVideos: IRequestHandler<ListVideosInput, ListVideosOutput>
    {
    }
}
