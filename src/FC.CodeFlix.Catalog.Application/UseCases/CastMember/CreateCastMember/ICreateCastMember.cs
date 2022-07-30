using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;

public interface ICreateCastMember: IRequestHandler<CreateCastMemberInput, CastMemberModelOutput>
{

}
