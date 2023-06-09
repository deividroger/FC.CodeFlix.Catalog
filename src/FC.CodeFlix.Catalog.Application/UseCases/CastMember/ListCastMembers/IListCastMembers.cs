
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;


public interface IListCastMembers : IRequestHandler<ListCastMembersInput, ListCastMembersOutput>
{

}
