using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;

public class UpdateCastMember : IUpdateCastMember
{
    private readonly ICastMemberRepository _castMemberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCastMember(ICastMemberRepository  castMemberRepository, IUnitOfWork unitOfWork)
    {
        _castMemberRepository = castMemberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CastMemberModelOutput> Handle(UpdateCastMemberInput input, CancellationToken cancellationToken)
    {
        var castMember = await _castMemberRepository.Get(input.Id, cancellationToken);

        castMember.Update(input.Name, input.Type);

        await _castMemberRepository.Update(castMember, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return CastMemberModelOutput.FromCastMember(castMember);
    }
}
