using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;

public interface IGetCastMember:
        IRequestHandler<GetCastMemberInput, CastMemberModelOutput>
{

}
