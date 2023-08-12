using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using GenreCreateGenre = FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest
{
    private readonly CreateGenreTestFixture _fixture;

    public CreateGenreTest(CreateGenreTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName =nameof(CreateGenre))]
    [Trait("Integration/Application", "CreateGenre - Use cases")]
    public async Task CreateGenre()
    {
        var input = _fixture.GetExampleInput();
        var actDbContext = _fixture.CreateDbContext();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var createGenre = new GenreCreateGenre.CreateGenre(new GenreRepository(actDbContext),
                                                           new UnitOfWork(actDbContext, eventPublisher, logger), 
                                                           new CategoryRepository(actDbContext));

        var output = await createGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default(DateTime));
        output.Categories.Should().HaveCount(0);

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreDb = await  assertDbContext.Genres.FindAsync(output.Id);

        genreDb.Should().NotBeNull();

        genreDb!.Name.Should().Be(output.Name);
        genreDb.Id.Should().NotBeEmpty();
        genreDb.IsActive.Should().Be(output.IsActive);  
        genreDb.CreatedAt.Should().NotBe(default(DateTime));
        genreDb.Categories.Should().HaveCount(0);



    }


    [Fact(DisplayName = nameof(CreateGenreWithCategoryRelations))]
    [Trait("Integration/Application", "CreateGenre - Use cases")]
    public async Task CreateGenreWithCategoryRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(5);
        

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.Categories.AddRangeAsync(exampleCategories);

        await arrangeDbContext.SaveChangesAsync();

        var input = _fixture.GetExampleInput();

        
        input.CategoriesIds = exampleCategories
                                    .Select(category => category.Id)
                                    .ToList();

        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var createGenre = new GenreCreateGenre.CreateGenre(new GenreRepository(actDbContext),
                                                           new UnitOfWork(actDbContext, eventPublisher, logger), 
                                                           new CategoryRepository(actDbContext));

        var output = await createGenre.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default(DateTime));
        output.Categories.Should().HaveCount(input.CategoriesIds.Count);

        output.Categories
              .Select(relation => relation.Id)
              .ToList()
              .Should()
              .BeEquivalentTo(input.CategoriesIds);

        var assertDbContext = _fixture.CreateDbContext(true);

        var genreDb = await assertDbContext.Genres.FindAsync(output.Id);

        genreDb.Should().NotBeNull();

        genreDb!.Name.Should().Be(output.Name);
        genreDb.Id.Should().NotBeEmpty();
        genreDb.IsActive.Should().Be(output.IsActive);

        var relations = await assertDbContext
                                    .GenresCategories
                                    .AsNoTracking()
                                    .Where(x => x.GenreId == genreDb.Id)
                                    .ToListAsync();

        relations.Should().HaveCount(input.CategoriesIds.Count);

        relations
                .Select(relation => relation.CategoryId)
                .ToList()
                .Should()
                .BeEquivalentTo(input.CategoriesIds);

    }


    [Fact(DisplayName = nameof(CreateGenreThrowsWhenCategoryDoesntExists))]
    [Trait("Integration/Application", "CreateGenre - Use cases")]
    public async Task CreateGenreThrowsWhenCategoryDoesntExists()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(5);

        var arrangeDbContext = _fixture.CreateDbContext();

        await arrangeDbContext.Categories.AddRangeAsync(exampleCategories);

        await arrangeDbContext.SaveChangesAsync();

        var input = _fixture.GetExampleInput();

        input.CategoriesIds = exampleCategories
                                    .Select(category => category.Id)
                                    .ToList();
        
        var randomGuid = Guid.NewGuid();

        input.CategoriesIds.Add(randomGuid);

        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var createGenre = new GenreCreateGenre.CreateGenre(new GenreRepository(actDbContext),
                                                           new UnitOfWork(actDbContext, eventPublisher, logger),
                                                           new CategoryRepository(actDbContext));

        var action = async  () => await createGenre.Handle(input, CancellationToken.None);

        await action.Should()
              .ThrowAsync<RelatedAggregateException>()
              .WithMessage($"Related category Id (or ids) not found : {randomGuid}");
       

    }
}