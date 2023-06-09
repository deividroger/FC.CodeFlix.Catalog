
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;

public interface IUpdateCastMember 
    : IRequestHandler<UpdateCastMemberInput,CastMemberModelOutput>
{
}
