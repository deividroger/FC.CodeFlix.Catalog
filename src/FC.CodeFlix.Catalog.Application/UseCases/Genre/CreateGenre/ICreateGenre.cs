using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;

public interface ICreateGenre : IRequestHandler<CreateGenreInput, GenreModelOutput>
{
}
