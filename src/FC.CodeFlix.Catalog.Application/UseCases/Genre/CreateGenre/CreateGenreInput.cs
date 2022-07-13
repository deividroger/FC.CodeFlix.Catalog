using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;

public class CreateGenreInput : IRequest<GenreModelOutput>
{
    public CreateGenreInput()
    {

    }

    public CreateGenreInput(string name, 
                            bool isActive,
                            List<Guid>? categoriesId = null)
    {
        Name = name;
        IsActive = isActive;

        CategoriesIds = categoriesId;
    }

    public string Name { get; set; }

    public bool IsActive { get; set; }

    public List<Guid>? CategoriesIds { get; set; }
}
