using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.Repository;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;

public class CreateGenre : ICreateGenre
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;

    public CreateGenre(IGenreRepository genreRepository, IUnitOfWork unitOfWork, ICategoryRepository categoryRepository)
        => (_genreRepository, _unitOfWork, _categoryRepository) = (genreRepository, unitOfWork, categoryRepository);

    public async Task<GenreModelOutput> Handle(CreateGenreInput request, CancellationToken cancellationToken)
    {
        var genre = new DomainEntity.Genre(request.Name, request.IsActive);


        if ((request.CategoriesIds?.Count ?? 0) > 0)
        {
            await ValidatedRequestId(request, cancellationToken);

            request.CategoriesIds!.ForEach(genre.AddCategory);
        }

        await _genreRepository.Insert(genre, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);


        return GenreModelOutput.FromGenre(genre);

    }

    private async Task ValidatedRequestId(CreateGenreInput request, CancellationToken cancellationToken)
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
