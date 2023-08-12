using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;
namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.UpdateGenre;


[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest
{
    private readonly UpdateGenreTestFixture _fixture;

    public UpdateGenreTest(UpdateGenreTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenre()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger ),
                                                    new CategoryRepository(actDbContext));

        var input = new UseCase.UpdateGenreInput(targetGenre.Id,
                                                 _fixture.GetValidGenreName(),
                                                 targetGenre.IsActive!);
        var output = await updateGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreFromDb = await assertDbContext.Genres.FindAsync(targetGenre.Id);

        genreFromDb.Should().NotBeNull();

        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

    }


    [Fact(DisplayName = nameof(UpdateGenreWithCategoriesRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithCategoriesRelations()
    {

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var relatedCategories = exampleCategories.GetRange(0, 5);
        var newRelatedCategories = exampleCategories.GetRange(5, 3);


        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));

        var relations = targetGenre
                            .Categories
                            .Select(categoryId =>
                                    new GenresCategories(categoryId, targetGenre.Id))
                            .ToList();

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.AddRangeAsync(exampleCategories);
        await arrageDbContext.AddRangeAsync(relations);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger),
                                                    new CategoryRepository(actDbContext));

        var input = new UseCase.UpdateGenreInput(targetGenre.Id,
                                                 _fixture.GetValidGenreName(),
                                                 targetGenre.IsActive!,
                                                 newRelatedCategories
                                                        .Select(categories =>categories.Id)
                                                            .ToList());

        var output = await updateGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);
        
        output.Categories.Should().HaveCount(newRelatedCategories.Count);

        var relatedCategoryIdsFromOutput =  output.Categories
                                                  .Select(relatedCategory => relatedCategory.Id)
                                                  .ToList();


        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(input.CategoriesIds);

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreFromDb = await assertDbContext.Genres.FindAsync(targetGenre.Id);

        genreFromDb.Should().NotBeNull();

        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

        var relatedCategoryIdFromDb = await assertDbContext
                                            .GenresCategories
                                            .AsNoTracking()
                                            .Where(x => x.GenreId == input.Id)
                                            .Select(y => y.CategoryId)
                                            .ToListAsync();

        relatedCategoryIdFromDb.Should().BeEquivalentTo(input.CategoriesIds);



    }


    [Fact(DisplayName = nameof(UpdateGenreThowsWhenCategoryDoesntExists))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreThowsWhenCategoryDoesntExists()
    {

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var relatedCategories = exampleCategories.GetRange(0, 5);
        var newRelatedCategories = exampleCategories.GetRange(5, 3);


        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));

        var relations = targetGenre
                            .Categories
                            .Select(categoryId =>
                                    new GenresCategories(categoryId, targetGenre.Id))
                            .ToList();

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.AddRangeAsync(exampleCategories);
        await arrageDbContext.AddRangeAsync(relations);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger),
                                                    new CategoryRepository(actDbContext));

        var categoryIdsToRelate = newRelatedCategories
                                                        .Select(categories => categories.Id)
                                                            .ToList();
        var invalidCategoryId = Guid.NewGuid();
        categoryIdsToRelate.Add(invalidCategoryId);

        var input = new UseCase.UpdateGenreInput(targetGenre.Id,
                                                 _fixture.GetValidGenreName(),
                                                 targetGenre.IsActive!,
                                                 categoryIdsToRelate);

        var action =  async () =>  await updateGenre.Handle(input, CancellationToken.None);


        await action.Should()
              .ThrowAsync<RelatedAggregateException>()
              .WithMessage($"Related category Id (or ids) not found : {invalidCategoryId}");

      
    }


    [Fact(DisplayName = nameof(UpdateGenreThrowsWhenNotFound))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreThrowsWhenNotFound()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger),
                                                    new CategoryRepository(actDbContext));

        var randomGuid = Guid.NewGuid();

        var input = new UseCase.UpdateGenreInput(randomGuid,
                                                 _fixture.GetValidGenreName(),
                                                 true);
        var action = async () => await updateGenre.Handle(input, CancellationToken.None);

        await action.Should()
                    .ThrowAsync<NotFoundException>()
                    .WithMessage($"Genre '{randomGuid}' not found.");

    }


    [Fact(DisplayName = nameof(UpdateGenreWithOutNewCategoriesRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithOutNewCategoriesRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var relatedCategories = exampleCategories.GetRange(0, 5);


        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));

        var relations = targetGenre
                            .Categories
                            .Select(categoryId =>
                                    new GenresCategories(categoryId, targetGenre.Id))
                            .ToList();

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.AddRangeAsync(exampleCategories);
        await arrageDbContext.AddRangeAsync(relations);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger),
                                                    new CategoryRepository(actDbContext));

        var input = new UseCase.UpdateGenreInput(targetGenre.Id,
                                                 _fixture.GetValidGenreName(),
                                                 !targetGenre.IsActive);

        var output = await updateGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);

        output.Categories.Should().HaveCount(relatedCategories.Count);

        var expectedRelatedCategoryIds = relatedCategories.Select(category => category.Id).ToList();

        var relatedCategoryIdsFromOutput = output.Categories
                                                  .Select(relatedCategory => relatedCategory.Id)
                                                  .ToList();


        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(expectedRelatedCategoryIds);

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreFromDb = await assertDbContext.Genres.FindAsync(targetGenre.Id);

        genreFromDb.Should().NotBeNull();

        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

        var relatedCategoryIdFromDb = await assertDbContext
                                            .GenresCategories
                                            .AsNoTracking()
                                            .Where(x => x.GenreId == input.Id)
                                            .Select(y => y.CategoryId)
                                            .ToListAsync();

        relatedCategoryIdFromDb.Should().BeEquivalentTo(expectedRelatedCategoryIds);



    }


    [Fact(DisplayName = nameof(UpdateGenreWithEmptyCategoriesIdsCleanRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithEmptyCategoriesIdsCleanRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var exampleGenres = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenres[5];

        var relatedCategories = exampleCategories.GetRange(0, 5);


        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));

        var relations = targetGenre
                            .Categories
                            .Select(categoryId =>
                                    new GenresCategories(categoryId, targetGenre.Id))
                            .ToList();

        var arrageDbContext = _fixture.CreateDbContext();

        await arrageDbContext.AddRangeAsync(exampleGenres);
        await arrageDbContext.AddRangeAsync(exampleCategories);
        await arrageDbContext.AddRangeAsync(relations);
        await arrageDbContext.SaveChangesAsync();


        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var updateGenre = new UseCase.UpdateGenre(new GenreRepository(actDbContext),
                                                    new UnitOfWork(actDbContext, eventPublisher, logger),
                                                    new CategoryRepository(actDbContext));

        var input = new UseCase.UpdateGenreInput(targetGenre.Id,
                                                 _fixture.GetValidGenreName(),
                                                 !targetGenre.IsActive,
                                                 new List<Guid>());

        var output = await updateGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);

        output.Categories.Should().HaveCount(0);


        var relatedCategoryIdsFromOutput = output.Categories
                                                  .Select(relatedCategory => relatedCategory.Id)
                                                  .ToList();


        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(new List<Guid>());

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreFromDb = await assertDbContext.Genres.FindAsync(targetGenre.Id);

        genreFromDb.Should().NotBeNull();

        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

        var relatedCategoryIdFromDb = await assertDbContext
                                            .GenresCategories
                                            .AsNoTracking()
                                            .Where(x => x.GenreId == input.Id)
                                            .Select(y => y.CategoryId)
                                            .ToListAsync();

        relatedCategoryIdFromDb.Should().BeEquivalentTo(new List<Guid>());



    }
}