
using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos;

public class ListVideosInput : PaginatedListInput, IRequest<ListVideosOutput>
{
    public ListVideosInput(int page = 1, int perPage = 15, string search = "", string sort = "", SearchOrder dir = SearchOrder.ASC)
        : base(page, perPage, search, sort, dir)
    {
    }
}
