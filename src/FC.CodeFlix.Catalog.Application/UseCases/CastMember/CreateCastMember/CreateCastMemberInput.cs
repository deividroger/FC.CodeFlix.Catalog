using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;

public class CreateCastMemberInput : IRequest<CastMemberModelOutput>
{
    public CreateCastMemberInput(string name, CastMemberType type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; private set; }

    public CastMemberType Type { get; private set; }
}
