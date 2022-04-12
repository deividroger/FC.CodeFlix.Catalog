namespace FC.CodeFlix.Catalog.Application.UseCases.Category.CreateCategory;

public class CreateCategoryOutput
{
    public CreateCategoryOutput(Guid id, string name, string description , DateTime createdAt, bool isActive = true)
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

}
