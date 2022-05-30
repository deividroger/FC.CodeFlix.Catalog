using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Domain.Validation;

namespace FC.CodeFlix.Catalog.Domain.Entity;

public class Genre : AggregateRoot

{

    private readonly List<Guid> _categories;

    public Genre(string name, bool isActive = true)
    {
        Name = name;
        IsActive = isActive;
        CreatedAt = DateTime.Now;
        _categories = new();
        Validate();
    }

    public IReadOnlyList<Guid> Categories => _categories.AsReadOnly();


    public string Name { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void Activate()
    {
        IsActive = true;
        Validate();
    }

    public void Deactivate()
    {
        IsActive = false;
        Validate();
    }

    public void Update(string newName)
    {
        Name = newName;
        Validate();
    }

    private void Validate()
        => DomainValidation
            .NotNullOrEmpty(Name, nameof(Name));

    public void AddCategory(Guid categoryId)
    {
        _categories.Add(categoryId);
        Validate();
    }

    public void RemoveCategory(Guid exampleGuid)
    {
        _categories.Remove(exampleGuid);

        Validate();
    }

    public void RemoveAllCategories()
    {
        _categories.Clear();
        Validate();
    }
}
