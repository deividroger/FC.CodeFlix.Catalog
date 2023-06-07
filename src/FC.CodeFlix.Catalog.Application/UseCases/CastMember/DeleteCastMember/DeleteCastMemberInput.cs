using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.DeleteCastMember;

public class DeleteCastMemberInput
    :IRequest
{
    public Guid Id { get; private set; }

    public DeleteCastMemberInput(Guid id) => Id = id;
    
}
