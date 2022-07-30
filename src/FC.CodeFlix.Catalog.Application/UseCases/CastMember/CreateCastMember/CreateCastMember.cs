using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Domain.Repository;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;

public class CreateCastMember : ICreateCastMember
{
    private readonly ICastMemberRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCastMember(ICastMemberRepository repository, IUnitOfWork unitOfWork)
        => (_repository, _unitOfWork) = (repository, unitOfWork);
    
    public async Task<CastMemberModelOutput> Handle(CreateCastMemberInput request, CancellationToken cancellationToken)
    {
        var castMember = new DomainEntity.CastMember(request.Name, request.Type);

        await _repository.Insert(castMember,cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return new CastMemberModelOutput(castMember.Id, castMember.Name, castMember.Type, castMember.CreatedAt);
    }
}