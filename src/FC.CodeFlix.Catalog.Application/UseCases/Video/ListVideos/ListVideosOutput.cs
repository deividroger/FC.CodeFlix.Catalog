using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos;

public class ListVideosOutput : PaginatedListOutput<VideoModelOutput>
{
    public ListVideosOutput(int page,   
                            int perPage, 
                            int total,
                            IReadOnlyList<VideoModelOutput> items) 
        : base(page, perPage, total, items)
    {}
}
