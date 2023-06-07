using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;

public class GetCastMemberInput: IRequest<CastMemberModelOutput>
{
    public GetCastMemberInput(Guid id)
        => Id = id;

    public Guid Id { get; private set; }
}
