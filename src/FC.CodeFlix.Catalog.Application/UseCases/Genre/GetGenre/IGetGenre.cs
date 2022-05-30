using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.GetGenre;

public interface IGetGenre: IRequestHandler<GetGenreInput,GenreModelOutput>
{
}
