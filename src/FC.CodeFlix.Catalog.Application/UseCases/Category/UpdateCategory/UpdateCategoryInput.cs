using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;

public class UpdateCategoryInput : IRequest<CategoryModelOutput>
{
    public UpdateCategoryInput(Guid id, string name, string description, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Description {  get; private set; }

    public bool IsActive { get; private set; }

}
