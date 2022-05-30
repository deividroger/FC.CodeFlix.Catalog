using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;

public class ListGenresInput : PaginatedListInput,IRequest<ListGenresOutput>
{
    public ListGenresInput(int page = 1, int perPage = 15, string search  ="", string sort = "", SearchOrder dir = SearchOrder.ASC) 
        : base(page, perPage, search, sort, dir)
    {
    }
}