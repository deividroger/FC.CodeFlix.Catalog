using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;

public class ListCastMembersInput
    :PaginatedListInput, IRequest<ListCastMembersOutput>
{
    public ListCastMembersInput(int page, int perPage, string search, string sort, SearchOrder dir) 
        : base(page, perPage, search, sort, dir)
    {
    }
}
