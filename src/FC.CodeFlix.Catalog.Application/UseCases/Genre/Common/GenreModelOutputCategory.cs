namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;

public class GenreModelOutputCategory
{
    public GenreModelOutputCategory(Guid id, string? name = null)
        => (Id, Name) = (id, name);
    

    public Guid Id { get; set; }

    public string? Name { get; set; }
}