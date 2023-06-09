
using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using DomainEntity= FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;

public class ListCastMembersOutput : PaginatedListOutput<CastMemberModelOutput>
{
    public ListCastMembersOutput(int page, int perPage, int total, IReadOnlyList<CastMemberModelOutput> items) 
        : base(page, perPage, total, items)
    {
    }
    public static ListCastMembersOutput FromSearchOutput(SearchOutput<DomainEntity.CastMember> searchOutput)
     => new (searchOutput.CurrentPage,
                                         searchOutput.PerPage,
                                         searchOutput.Total,
                                         searchOutput.Items
                                            .Select(castMember => CastMemberModelOutput
                                            .FromCastMember(castMember))
                                            .ToList()
                                        );
}
