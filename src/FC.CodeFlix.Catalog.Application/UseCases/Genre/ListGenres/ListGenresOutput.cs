using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;

public class ListGenresOutput : PaginatedListOutput<GenreModelOutput>
{
    public ListGenresOutput(int page, int perPage, int total, IReadOnlyList<GenreModelOutput> items)
        : base(page, perPage, total, items)
    {
    }

    public static ListGenresOutput FromSearchOutput(SearchOutput<DomainEntity.Genre> searchOutput)
     => new(searchOutput.CurrentPage,
            searchOutput.PerPage,
            searchOutput.Total,
            searchOutput.Items
                 .Select(GenreModelOutput.FromGenre)
                 .ToList());

    public void FillWithCategoryNames(IReadOnlyList<DomainEntity.Category> categories)
    {
        foreach (var item in Items)
        {
            foreach (var categoryOutput in item.Categories)
            {
                categoryOutput.Name = categories?.FirstOrDefault(x => x.Id == categoryOutput.Id)?.Name;
            }
        }
    }
}