using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.GetGenre;

public class GetGenre : IGetGenre
{
    private readonly IGenreRepository _genreRepository;

    public GetGenre(IGenreRepository genreRepository)
        => _genreRepository = genreRepository;

    public async Task<GenreModelOutput> Handle(GetGenreInput request, CancellationToken cancellationToken)
    {
        var genre = await _genreRepository.Get(request.Id,cancellationToken);

        return GenreModelOutput.FromGenre(genre);
    }
}
