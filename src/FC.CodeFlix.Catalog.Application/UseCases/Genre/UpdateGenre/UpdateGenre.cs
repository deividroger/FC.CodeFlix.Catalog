using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;

public class UpdateGenre : IUpdateGenre
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _ofWork;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateGenre(IGenreRepository genreRepository, IUnitOfWork ofWork, ICategoryRepository categoryRepository)
        => (_genreRepository, _ofWork, _categoryRepository) = (genreRepository, ofWork, categoryRepository);


    public async Task<GenreModelOutput> Handle(UpdateGenreInput request, CancellationToken cancellationToken)
    {
        var genre = await _genreRepository.Get(request.Id, cancellationToken);

        genre.Update(request.Name);
        if (request.IsActive is not null && request.IsActive != genre.IsActive)
        {
            if ((bool)request.IsActive) genre.Activate();
            else genre.Deactivate();
        }

        if (request.CategoriesIds is not null)
        {
            genre.RemoveAllCategories();

            if (request.CategoriesIds.Count > 0)
            {
                await ValidatedRequestId(request, cancellationToken);
                    
                request.CategoriesIds?.ForEach(genre.AddCategory);
            }
        }

        await _genreRepository.Update(genre, cancellationToken);
        await _ofWork.Commit(cancellationToken);

        return GenreModelOutput.FromGenre(genre);
    }


    private async Task ValidatedRequestId(UpdateGenreInput request, CancellationToken cancellationToken)
    {
        var idsInPersistence = await _categoryRepository.GetIdsListByIds(request.CategoriesIds!, cancellationToken);

        if (idsInPersistence.Count < request.CategoriesIds!.Count)
        {
            var notFoundId = request.CategoriesIds
                                    .FindAll(x => !idsInPersistence.Contains(x));
            var notFoundIdsAsString = string.Join(", ", notFoundId);

            throw new RelatedAggregateException($"Related category Id (or ids) not found : {notFoundIdsAsString}");
        }

    }
}
