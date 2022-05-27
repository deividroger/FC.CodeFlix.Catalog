using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;

public class UpdateGenreInput : IRequest<GenreModelOutput>
{
    public UpdateGenreInput(Guid id, string name, bool? isActive = null, List<Guid>? categoriesIds = null)
    {
        Name = name;
        IsActive = isActive;
        Id = id;
        CategoriesIds = categoriesIds;
    }

    public string Name { get; set; }

    public bool? IsActive { get; set; }
    public Guid Id { get; set; }

    public List<Guid>? CategoriesIds { get; set; }
}
