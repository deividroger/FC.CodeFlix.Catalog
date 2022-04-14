using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.Application.UseCases.Category.Common;

public class CategoryModelOutput
{
    public CategoryModelOutput(Guid id, string name, string description, DateTime createdAt, bool isActive = true)
    {
        Name = name;
        Id = id;
        CreatedAt = createdAt;

        Description = description;
        IsActive = isActive;
    }

    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

    public static CategoryModelOutput FromCategory(DomainEntity.Category category)
     => new(category.Id,
                                            category.Name,
                                            category.Description,
                                            category.CreatedAt,
                                            category.IsActive);
}


