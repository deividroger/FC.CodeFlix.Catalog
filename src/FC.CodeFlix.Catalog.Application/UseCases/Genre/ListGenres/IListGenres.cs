using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;

public interface IListGenres : IRequestHandler<ListGenresInput, ListGenresOutput>
{
}
