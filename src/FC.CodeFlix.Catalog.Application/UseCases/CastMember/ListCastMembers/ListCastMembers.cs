using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;

public class ListCastMembers : IListCastMembers
{
    private readonly ICastMemberRepository _castMemberRepository;

    public ListCastMembers(ICastMemberRepository castMemberRepository)
        => _castMemberRepository = castMemberRepository;


    public async Task<ListCastMembersOutput> Handle(ListCastMembersInput input, CancellationToken cancellationToken)
    {
        var searchOutput = await _castMemberRepository.Search(input.ToSearchInput(), cancellationToken);
        
        return  ListCastMembersOutput.FromSearchOutput(searchOutput);
    }
}
