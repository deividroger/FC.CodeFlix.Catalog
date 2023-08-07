using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Domain.Validation;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Repository = FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
namespace FC.CodeFlix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.GenreRepository;

[Collection(nameof(GenreRepositoryTestFixture))]
public class GenreRepositoryTest
{
    private readonly GenreRepositoryTestFixture _fixture;

    public GenreRepositoryTest(GenreRepositoryTestFixture fixture)
         => _fixture = fixture;

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        var genreRepository = new Repository.GenreRepository(dbContext);
        await dbContext.SaveChangesAsync(CancellationToken.None);


        await genreRepository.Insert(exampleGenre, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);

        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);


        var genresCategoriesRelation = await assertsDbContext
                                                .GenresCategories
                                                .Where(r => r.GenreId == exampleGenre.Id)
                                                .ToListAsync();

        genresCategoriesRelation.Should().HaveCount(categoriesListExample.Count);

        genresCategoriesRelation.ForEach(relation =>
        {

            var expectedCategory = categoriesListExample
                                    .FirstOrDefault(x => x.Id == relation.CategoryId);

            expectedCategory.Should().NotBeNull();
        });
    }


    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task Get()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var genreRepository = new Repository.GenreRepository(_fixture.CreateDbContext(true));


        var genreFromRepository = await genreRepository.Get(exampleGenre.Id, CancellationToken.None);


        genreFromRepository.Should().NotBeNull();
        genreFromRepository!.Name.Should().Be(exampleGenre.Name);
        genreFromRepository.IsActive.Should().Be(exampleGenre.IsActive);
        genreFromRepository.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        genreFromRepository.Categories.Should().HaveCount(categoriesListExample.Count);

        foreach (var categoryId in genreFromRepository.Categories)
        {
            var expectedCategory = categoriesListExample
                                    .FirstOrDefault(x => x.Id == categoryId);

            expectedCategory.Should().NotBeNull();
        }
    }


    [Fact(DisplayName = nameof(GetThrowWhenNotFound))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetThrowWhenNotFound()
    {
        var exampleNotFoundGuid = Guid.NewGuid();

        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var genreRepository = new Repository.GenreRepository(_fixture.CreateDbContext(true));

        var action = async () => await genreRepository.Get(exampleNotFoundGuid, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{exampleNotFoundGuid}' not found.");

    }


    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task Delete()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repositoryDbContext = _fixture.CreateDbContext(true);

        var genreRepository = new Repository.GenreRepository(repositoryDbContext);


        await genreRepository.Delete(exampleGenre, CancellationToken.None);

        await repositoryDbContext.SaveChangesAsync(CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);

        var dbGenre = assertDbContext.Genres.AsNoTracking().FirstOrDefault(x => x.Id == exampleGenre.Id);

        dbGenre.Should().BeNull();

        var categoriesIdsList = await assertDbContext
                                    .GenresCategories
                                    .AsNoTracking()
                                    .Where(x => x.GenreId == exampleGenre.Id)
                                    .Select(x => x.CategoryId)
                                    .ToListAsync(CancellationToken.None);

        categoriesIdsList.Should().HaveCount(0);


    }


    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task Update()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);
        exampleGenre.Update(_fixture.GetValidGenreName());
        if (exampleGenre.IsActive)
            exampleGenre.Deactivate();
        else
            exampleGenre.Activate();

        await genreRepository.Update(exampleGenre, CancellationToken.None);

        await actDbContext.SaveChangesAsync(CancellationToken.None);


        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);

        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);


        var genresCategoriesRelation = await assertsDbContext
                                                .GenresCategories
                                                .Where(r => r.GenreId == exampleGenre.Id)
                                                .ToListAsync();

        genresCategoriesRelation.Should().HaveCount(categoriesListExample.Count);

        genresCategoriesRelation.ForEach(relation =>
        {
            var expectedCategory = categoriesListExample
                                    .FirstOrDefault(x => x.Id == relation.CategoryId);

            expectedCategory.Should().NotBeNull();
        });

    }


    [Fact(DisplayName = nameof(UpdateRemovingRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task UpdateRemovingRelations()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);
        exampleGenre.Update(_fixture.GetValidGenreName());
        if (exampleGenre.IsActive)
            exampleGenre.Deactivate();
        else
            exampleGenre.Activate();

        exampleGenre.RemoveAllCategories();

        await genreRepository.Update(exampleGenre, CancellationToken.None);

        await actDbContext.SaveChangesAsync(CancellationToken.None);


        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);

        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);


        var genresCategoriesRelation = await assertsDbContext
                                                .GenresCategories
                                                .Where(r => r.GenreId == exampleGenre.Id)
                                                .ToListAsync();

        genresCategoriesRelation.Should().HaveCount(0);


    }


    [Fact(DisplayName = nameof(UpdateReplacingRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task UpdateReplacingRelations()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList(3);
        var updateCategoriesListExample = _fixture.GetExampleCategoriesList(2);

        categoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample);
        await dbContext.Categories.AddRangeAsync(updateCategoriesListExample);
        await dbContext.Genres.AddAsync(exampleGenre);


        foreach (var categoryId in exampleGenre.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);

            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);
        exampleGenre.Update(_fixture.GetValidGenreName());

        if (exampleGenre.IsActive)
            exampleGenre.Deactivate();
        else
            exampleGenre.Activate();

        exampleGenre.RemoveAllCategories();

        updateCategoriesListExample.ForEach(category => exampleGenre.AddCategory(category.Id));

        await genreRepository.Update(exampleGenre, CancellationToken.None);

        await actDbContext.SaveChangesAsync(CancellationToken.None);


        var assertsDbContext = _fixture.CreateDbContext(true);

        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);

        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);


        var genresCategoriesRelation = await assertsDbContext
                                               .GenresCategories
                                               .Where(r => r.GenreId == exampleGenre.Id)
                                               .ToListAsync();

        genresCategoriesRelation.Should().HaveCount(updateCategoriesListExample.Count);

        genresCategoriesRelation.ForEach(relation =>
        {
            var expectedCategory = updateCategoriesListExample
                                    .FirstOrDefault(x => x.Id == relation.CategoryId);

            expectedCategory.Should().NotBeNull();
        });


    }


    [Fact(DisplayName = nameof(SearchReturnsItemsAndTotal))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchReturnsItemsAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenresList = _fixture.GetExampleListGenres(10);

        await dbContext.Genres.AddRangeAsync(exampleGenresList);

        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);



        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);

        searchResult.Items.Should().HaveCount(exampleGenresList.Count);


        foreach (var item in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x=>x.Id == item.Id);

            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(exampleGenre.Name);
            exampleGenre.IsActive.Should().Be(exampleGenre.IsActive);
            exampleGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        }
    }


    [Fact(DisplayName = nameof(SearchReturnsRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchReturnsRelations()
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenresList = _fixture.GetExampleListGenres(10);

        var random = new Random();
        exampleGenresList.ForEach(async exampleGenre => {
            var categoriesListToRelation = _fixture
                                                .GetExampleCategoriesList (random.Next(0, 4));

            if(categoriesListToRelation.Count > 0)
            {
                categoriesListToRelation.ForEach(category => {
                    exampleGenre.AddCategory(category.Id);
                });

                 await dbContext.Categories.AddRangeAsync(categoriesListToRelation);

                var relationsToAdd = categoriesListToRelation
                                        .Select(category => new GenresCategories(category.Id, exampleGenre.Id));
                await dbContext.GenresCategories.AddRangeAsync(relationsToAdd);
            }

        });

        dbContext.SaveChanges();

        await dbContext.Genres.AddRangeAsync(exampleGenresList);

        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);

        searchResult.Items.Should().HaveCount(exampleGenresList.Count);


        foreach (var item in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == item.Id);

            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(exampleGenre.Name);
            exampleGenre.IsActive.Should().Be(exampleGenre.IsActive);
            exampleGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

            exampleGenre.Categories.Should().HaveCount(exampleGenre.Categories.Count);
            exampleGenre.Categories.Should().BeEquivalentTo(exampleGenre.Categories);

        }
    }

    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenPersistenceIsEmpty()
    {
        

   
        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.ASC);

        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(0);

        searchResult.Items.Should().HaveCount(0);


    }


    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(int quantityToGenerate,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItems)
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenresList = _fixture.GetExampleListGenres(quantityToGenerate);

        var random = new Random();
        exampleGenresList.ForEach(async exampleGenre => {
            var categoriesListToRelation = _fixture
                                                .GetExampleCategoriesList(random.Next(0, 4));

            if (categoriesListToRelation.Count > 0)
            {
                categoriesListToRelation.ForEach(category => {
                    exampleGenre.AddCategory(category.Id);
                });

                await dbContext.Categories.AddRangeAsync(categoriesListToRelation);

                var relationsToAdd = categoriesListToRelation
                                        .Select(category => new GenresCategories(category.Id, exampleGenre.Id));
                await dbContext.GenresCategories.AddRangeAsync(relationsToAdd);
            }

        });

        dbContext.SaveChanges();

        await dbContext.Genres.AddRangeAsync(exampleGenresList);

        await dbContext.SaveChangesAsync(CancellationToken.None);



        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);

        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.ASC);

        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);

        searchResult.Items.Should().HaveCount(expectedQuantityItems);


        foreach (var item in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == item.Id);

            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(exampleGenre.Name);
            exampleGenre.IsActive.Should().Be(exampleGenre.IsActive);
            exampleGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

            exampleGenre.Categories.Should().HaveCount(exampleGenre.Categories.Count);
            exampleGenre.Categories.Should().BeEquivalentTo(exampleGenre.Categories);

        }
    }



    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    [InlineData("action", 1, 5, 1, 1)]
    [InlineData("horror", 1, 5, 3, 3)]
    [InlineData("horror", 2, 5, 0, 3)]
    [InlineData("sci-fi", 1, 5, 4, 4)]
    [InlineData("sci-fi", 1, 2, 2, 4)]
    [InlineData("sci-fi", 2, 3, 1, 4)]
    [InlineData("sci-fi other", 1, 3, 0, 0)]
    [InlineData("robots", 1, 5, 2, 2)]
    public async Task SearchByText(string search,
                                             int page,
                                             int perPage,
                                             int expectedQuantityItemsReturned,
                                             int expectedQuantityTotalItems)
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenresList = _fixture.GetExampleListGenresByNames(new List<string>() {
           "action",
           "horror",
           "horror - robots",
           "horror - bases on real facts",
           "drama",
           "sci-fi IA",
           "sci-fi Space",
           "sci-fi robots",
           "sci-fi future",
        });

        await dbContext.Genres.AddRangeAsync(exampleGenresList);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var random = new Random();
        exampleGenresList.ForEach(async exampleGenre => {
            var categoriesListToRelation = _fixture
                                                .GetExampleCategoriesList(random.Next(0, 4));

            if (categoriesListToRelation.Count > 0)
            {
                categoriesListToRelation.ForEach(category => {
                    exampleGenre.AddCategory(category.Id);
                });

                await dbContext.Categories.AddRangeAsync(categoriesListToRelation);

                var relationsToAdd = categoriesListToRelation
                                        .Select(category => new GenresCategories(category.Id, exampleGenre.Id));
                await dbContext.GenresCategories.AddRangeAsync(relationsToAdd);
            }

        });

        dbContext.SaveChanges();

        await dbContext.SaveChangesAsync(CancellationToken.None);


        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Repository.GenreRepository(actDbContext);

        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.ASC);

        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(expectedQuantityTotalItems);

        searchResult.Items.Should().HaveCount(expectedQuantityItemsReturned);


        foreach (var item in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == item.Id);

            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(exampleGenre.Name);
            exampleGenre.IsActive.Should().Be(exampleGenre.IsActive);
            exampleGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

            exampleGenre.Categories.Should().HaveCount(exampleGenre.Categories.Count);
            exampleGenre.Categories.Should().BeEquivalentTo(exampleGenre.Categories);

        }
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]

    [InlineData("id", "asc")]
    [InlineData("id", "desc")]

    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]

    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = _fixture.CreateDbContext();

        var exampleGenreList = _fixture.GetExampleListGenres(10);

        await dbContext.AddRangeAsync(exampleGenreList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new Repository.GenreRepository(dbContext);

        var searchOrder = order.ToLower() == "asc" ? SearchOrder.ASC : SearchOrder.DESC;

        var searhInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var output = await genreRepository.Search(searhInput, CancellationToken.None);

        var expectedOrderList = _fixture.CloneGenresListOrdered(exampleGenreList, orderBy, searchOrder);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searhInput.Page);
        output.PerPage.Should().Be(searhInput.PerPage);
        output.Total.Should().Be(exampleGenreList.Count);
        output.Items.Should().HaveCount(exampleGenreList.Count);

        for (int i = 0; i < expectedOrderList.Count; i++)
        {
            var expectedItem = expectedOrderList[i];

            var outputItem = output.Items[i];

            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();

            outputItem.Name.Should().Be(expectedItem!.Name);
            outputItem.Id.Should().Be(expectedItem!.Id);

            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(GetIdsListByIds))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetIdsListByIds()
    {
        var arrageDbContext = _fixture.CreateDbContext();

        var exampleGenreList = _fixture.GetExampleListGenres(10);

        await arrageDbContext.AddRangeAsync(exampleGenreList);
        await arrageDbContext.SaveChangesAsync(CancellationToken.None);

        var actDbContext = _fixture.CreateDbContext(true);

        var genreRepository = new Repository.GenreRepository(actDbContext);

        var idsToGet = new List<Guid> {
            exampleGenreList[2].Id,
            exampleGenreList[3].Id,
        };

        var result = await genreRepository.GetIdsListByIds(idsToGet, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(idsToGet.Count);
        result.Should().BeEquivalentTo(idsToGet);
    }


    [Fact(DisplayName = nameof(GetIdsListByIdWhenOnlyThreeIdsMatchs))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetIdsListByIdWhenOnlyThreeIdsMatchs()
    {
        var arrageDbContext = _fixture.CreateDbContext();

        var exampleGenreList = _fixture.GetExampleListGenres(10);

        await arrageDbContext.AddRangeAsync(exampleGenreList);
        await arrageDbContext.SaveChangesAsync(CancellationToken.None);

        var actDbContext = _fixture.CreateDbContext(true);

        var genreRepository = new Repository.GenreRepository(actDbContext);

        var idsToGet = new List<Guid> {
            exampleGenreList[3].Id,
            exampleGenreList[4].Id,
            exampleGenreList[5].Id,
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var idsExpectedToReturn = new List<Guid> {
            exampleGenreList[3].Id,
            exampleGenreList[4].Id,
            exampleGenreList[5].Id
        };


        var result = await genreRepository.GetIdsListByIds(idsToGet, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().NotBeEquivalentTo(idsToGet);
        result.Should().BeEquivalentTo(idsExpectedToReturn);    
    }


    [Fact(DisplayName = nameof(GetListByIds))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetListByIds()
    {
        var arrageDbContext = _fixture.CreateDbContext();

        var exampleGenreList = _fixture.GetExampleListGenres(10);

        await arrageDbContext.AddRangeAsync(exampleGenreList);
        await arrageDbContext.SaveChangesAsync(CancellationToken.None);

        var actDbContext = _fixture.CreateDbContext(true);

        var genreRepository = new Repository.GenreRepository(actDbContext);

        var idsToGet = new List<Guid> {
            exampleGenreList[3].Id,
            exampleGenreList[4].Id,
            exampleGenreList[5].Id,
        };

        var result = await genreRepository.GetListByIds(idsToGet, CancellationToken.None);

        result.Should().NotBeNull();

        result.Should().HaveCount(idsToGet.Count);

        idsToGet.ForEach(id =>
        {
            var example = exampleGenreList.FirstOrDefault(x => x.Id == id);
            var resultItem = result.FirstOrDefault(x => x.Id == id);
            example.Should().NotBeNull();
            resultItem.Should().NotBeNull();
            resultItem!.Name.Should().Be(example!.Name);
            resultItem!.Id.Should().Be(example!.Id);
            resultItem!.IsActive.Should().Be(example!.IsActive);
            resultItem!.CreatedAt.Should().Be(example!.CreatedAt);
        });
    }

    [Fact(DisplayName = nameof(GetListByIdsdWhenOnlyThreeIdsMatchs))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetListByIdsdWhenOnlyThreeIdsMatchs()
    {
        var arrageDbContext = _fixture.CreateDbContext();

        var exampleGenreList = _fixture.GetExampleListGenres(10);

        await arrageDbContext.AddRangeAsync(exampleGenreList);
        await arrageDbContext.SaveChangesAsync(CancellationToken.None);

        var actDbContext = _fixture.CreateDbContext(true);

        var genreRepository = new Repository.GenreRepository(actDbContext);

        var idsToGet = new List<Guid> {
            exampleGenreList[3].Id,
            exampleGenreList[4].Id,
            exampleGenreList[5].Id,
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var idsExpectedToGet = new List<Guid> {
            exampleGenreList[3].Id,
            exampleGenreList[4].Id,
            exampleGenreList[5].Id
        };


        var result = await genreRepository.GetListByIds(idsToGet, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(idsExpectedToGet.Count);
        result.Should().NotBeEquivalentTo(idsToGet);


        idsExpectedToGet.ForEach(id =>
        {
            var example = exampleGenreList.FirstOrDefault(x => x.Id == id);
            var resultItem = result.FirstOrDefault(x => x.Id == id);
            example.Should().NotBeNull();
            resultItem.Should().NotBeNull();
            resultItem!.Name.Should().Be(example!.Name);
            resultItem!.Id.Should().Be(example!.Id);
            resultItem!.IsActive.Should().Be(example!.IsActive);
            resultItem!.CreatedAt.Should().Be(example!.CreatedAt);
        });
    }
}