using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;
namespace FC.CodeFlix.Catalog.UnitTests.Application.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest
{
    private readonly ListGenresTestFixture _fixture;

    public ListGenresTest(ListGenresTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Application", "ListGenres - Use Cases")]
    public async Task ListGenres()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();

        var genresListExample = _fixture.GetExampleGenresList();

        var input = _fixture.GetExampleInput();

        var outputRepositorySearch = new SearchOutput<DomainEntity.Genre>(
         currentPage: input.Page,
         perPage: input.PerPage,
         items: genresListExample.AsReadOnly(),
         total: new Random().Next(50, 200)
        );

        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(outputRepositorySearch);

        var useCase = new UseCase.ListGenres(genreRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputRepositorySearch.CurrentPage);
        output.PerPage.Should().Be(outputRepositorySearch.PerPage);
        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);
        output.Total.Should().Be(outputRepositorySearch.Total);

        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);
        ((List<GenreModelOutput>)output.Items).ForEach(outputItem =>
        {

            var repositoryGenre = outputRepositorySearch.Items.FirstOrDefault(x => x.Id == outputItem.Id);

            repositoryGenre.Should().NotBeNull();   
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(repositoryGenre!.Name);
            outputItem.IsActive.Should().Be(repositoryGenre!.IsActive);
            outputItem.CreatedAt.Should().Be(repositoryGenre!.CreatedAt);

            outputItem.Categories.Should().HaveCount(repositoryGenre.Categories.Count);

            foreach (var expectedId in repositoryGenre.Categories)
            {
                outputItem.Categories.Should().Contain(expectedId);
            }
        });

        genreRepositoryMock.Verify(
            x => x.Search(
                It.Is<SearchInput>(
                    searchInput =>
                    searchInput.Page == input.Page
                    && searchInput.PerPage == input.PerPage
                    && searchInput.Search == input.Search
                    && searchInput.OrderBy == input.Sort
                    && searchInput.Order == input.Dir
                    ),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

    }

    [Fact(DisplayName = nameof(ListEmpty))]
    [Trait("Application", "ListGenres - Use Cases")]
    public async Task ListEmpty()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();

        var input = _fixture.GetExampleInput();

        var outputRepositorySearch = new SearchOutput<DomainEntity.Genre>(
         currentPage: input.Page,
         perPage: input.PerPage,
         items: new List<DomainEntity.Genre>(),
         total: new Random().Next(50, 200)
        );

        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(outputRepositorySearch);

        var useCase = new UseCase.ListGenres(genreRepositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputRepositorySearch.CurrentPage);
        output.PerPage.Should().Be(outputRepositorySearch.PerPage);
        output.Total.Should().Be(outputRepositorySearch.Total);
        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);

        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);
 
        genreRepositoryMock.Verify(
            x => x.Search(
                It.Is<SearchInput>(
                    searchInput =>
                    searchInput.Page == input.Page
                    && searchInput.PerPage == input.PerPage
                    && searchInput.Search == input.Search
                    && searchInput.OrderBy == input.Sort
                    && searchInput.Order == input.Dir
                    ),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

    }

    [Fact(DisplayName = nameof(ListUsingDefaultInputValues))]
    [Trait("Application", "ListGenres - Use Cases")]
    public async Task ListUsingDefaultInputValues()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();

        var outputRepositorySearch = new SearchOutput<DomainEntity.Genre>(
         currentPage: 1,
         perPage: 15,
         items: new List<DomainEntity.Genre>(),
         total: 0
        );

        genreRepositoryMock.Setup(x => x.Search(It.IsAny<SearchInput>(),
                                             It.IsAny<CancellationToken>())
        ).ReturnsAsync(outputRepositorySearch);

        var useCase = new UseCase.ListGenres(genreRepositoryMock.Object);

        var output = await useCase.Handle(new UseCase.ListGenresInput(), CancellationToken.None);


        output.Items.Should().HaveCount(outputRepositorySearch.Items.Count);

        genreRepositoryMock.Verify(
            x => x.Search(
                It.Is<SearchInput>(
                    searchInput =>
                    searchInput.Page == 1
                    && searchInput.PerPage == 15
                    && searchInput.Search == ""
                    && searchInput.OrderBy ==  ""
                    && searchInput.Order ==  SearchOrder.ASC
                    ),
                It.IsAny<CancellationToken>()
                )
            , Times.Once());

    }
}
