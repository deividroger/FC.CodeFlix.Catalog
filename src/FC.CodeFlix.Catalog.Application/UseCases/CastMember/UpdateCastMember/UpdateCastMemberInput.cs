using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;

public class UpdateCastMemberInput : IRequest<CastMemberModelOutput>
{
    public Guid Id  { get; set; }

    public string Name { get; set; }

    public CastMemberType Type { get; set; }

    public UpdateCastMemberInput(Guid id, string name, CastMemberType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }
}
