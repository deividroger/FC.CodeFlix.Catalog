using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.DeleteCastMember;

public class DeleteCastMember : IDeleteCastMember
{
    private readonly ICastMemberRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCastMember(ICastMemberRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCastMemberInput request, CancellationToken cancellationToken)
    {
        var castMember = await _repository.Get(request.Id, cancellationToken);

        await _repository.Delete(castMember, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Unit.Value;
        
    }
}
