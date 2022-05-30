using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre;

public interface IDeleteGenre: IRequestHandler<DeleteGenreInput>
{
}
