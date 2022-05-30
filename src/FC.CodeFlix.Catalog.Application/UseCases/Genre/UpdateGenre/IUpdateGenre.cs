using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;

public interface IUpdateGenre : IRequestHandler<UpdateGenreInput, GenreModelOutput>
{
}
