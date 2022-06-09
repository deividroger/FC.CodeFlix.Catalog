using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;

public class ListGenres : IListGenres
{
    private readonly IGenreRepository _genreRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ListGenres(IGenreRepository genreRepository, ICategoryRepository categoryRepository)
        => (_genreRepository, _categoryRepository) = (genreRepository, categoryRepository);

    public async Task<ListGenresOutput> Handle(ListGenresInput request, CancellationToken cancellationToken)
    {
        var searchOutput = await _genreRepository.Search(request.ToSearchInput(), cancellationToken);

        var output = ListGenresOutput.FromSearchOutput(searchOutput);

        var relatedCategories = searchOutput.Items
                                .SelectMany(item => item.Categories)
                                .Distinct()
                                .ToList();

        if (relatedCategories.Count > 0)
        {

            var categories = await _categoryRepository.GetListByIds(relatedCategories,
                                                                    cancellationToken);

            output.FillWithCategoryNames(categories);
        }
        
        return output;
    }

}