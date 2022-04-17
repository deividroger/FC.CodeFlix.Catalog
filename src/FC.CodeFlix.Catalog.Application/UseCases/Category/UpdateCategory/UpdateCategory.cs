using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;

public class UpdateCategory : IUpdateCategory
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategory(ICategoryRepository repository, IUnitOfWork unitOfWork)
        => (_repository, _unitOfWork) = (repository, unitOfWork);

    public async Task<CategoryModelOutput> Handle(UpdateCategoryInput request, CancellationToken cancellationToken)
    {
        var category = await _repository.Get(request.Id, cancellationToken);

        category.Update(request.Name, request.Description);

        if (request.IsActive != category.IsActive)
            if (request.IsActive) category.Active();
            else category.Deativate();

        await _repository.Update(category, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return CategoryModelOutput.FromCategory(category);

    }
}
