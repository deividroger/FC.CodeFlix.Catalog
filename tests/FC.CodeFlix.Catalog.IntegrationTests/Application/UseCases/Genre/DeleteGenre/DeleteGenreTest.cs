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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre;
namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Genre.DeleteGenre
{
    [Collection(nameof(DeleteGenreTestFixture))]
    public class DeleteGenreTest
    {
        private readonly DeleteGenreTestFixture _fixture;

        public DeleteGenreTest(DeleteGenreTestFixture fixture)
            => _fixture = fixture;
        
        [Fact(DisplayName =nameof(DeleteGenre))]
        [Trait("Integration/Application", "DeleteGenre - Use Cases")]
        public async Task DeleteGenre() {

            var genresExamplesList = _fixture.GetExampleListGenres(10);

            var targetGenre = genresExamplesList[5];
            var dbContext = _fixture.CreateDbContext();
            await dbContext.AddRangeAsync(genresExamplesList);
            dbContext.SaveChanges();

            var actDbContext = _fixture.CreateDbContext(true);

            var repository = new GenreRepository(actDbContext);

            var input = new UseCase.DeleteGenreInput(targetGenre.Id);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventPublisher = new DomainEventPublisher(serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

            var useCase = new UseCase.DeleteGenre(repository, new UnitOfWork(actDbContext, eventPublisher, logger));

             await useCase.Handle(input,CancellationToken.None);

            var dbContextAssert = _fixture.CreateDbContext(true);

            var genreFromDb = await dbContextAssert.Genres.FindAsync(targetGenre.Id);

            genreFromDb.Should().BeNull();

        }

        [Fact(DisplayName = nameof(DeleteGenre))]
        [Trait("Integration/Application", "DeleteGenre - Use Cases")]
        public async Task DeleteGenreThrowsWhenNotFound()
        {
            var genresExamplesList = _fixture.GetExampleListGenres(10);

            var dbContext = _fixture.CreateDbContext();
            await dbContext.AddRangeAsync(genresExamplesList);
            dbContext.SaveChanges();

            var actDbContext = _fixture.CreateDbContext(true);

            var repository = new GenreRepository(actDbContext);

            var randomGuid = Guid.NewGuid();

            var input = new UseCase.DeleteGenreInput(randomGuid);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventPublisher = new DomainEventPublisher(serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

            var useCase = new UseCase.DeleteGenre(repository, new UnitOfWork(actDbContext, eventPublisher, logger));

            var action =  async () =>  await useCase.Handle(input, CancellationToken.None);


            await action.Should()
                  .ThrowAsync<NotFoundException>()
                  .WithMessage($"Genre '{randomGuid}' not found.");

        }

        [Fact(DisplayName = nameof(DeleteGenreWithRelations))]
        [Trait("Integration/Application", "DeleteGenre - Use Cases")]
        public async Task DeleteGenreWithRelations()
        {
            var genresExamplesList = _fixture.GetExampleListGenres(10);

            var targetGenre = genresExamplesList[5];

            var exampleCategories = _fixture.GetExampleCategoriesList(5);

            var dbContext = _fixture.CreateDbContext();
            await dbContext.AddRangeAsync(genresExamplesList);
            await dbContext.Categories.AddRangeAsync(exampleCategories);

            await dbContext.GenresCategories.AddRangeAsync(
                exampleCategories
                    .Select(category => 
                        new GenresCategories(category.Id,targetGenre.Id))
                            );

            dbContext.SaveChanges();

            var actDbContext = _fixture.CreateDbContext(true);

            var repository = new GenreRepository(actDbContext);

            var input = new UseCase.DeleteGenreInput(targetGenre.Id);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventPublisher = new DomainEventPublisher(serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

            var useCase = new UseCase.DeleteGenre(repository, new UnitOfWork(actDbContext, eventPublisher, logger));

            await useCase.Handle(input, CancellationToken.None);

            var dbContextAssert = _fixture.CreateDbContext(true);

            var genreFromDb = await dbContextAssert.Genres.FindAsync(targetGenre.Id);

            genreFromDb.Should().BeNull();

            var relations = await dbContextAssert
                                        .GenresCategories
                                        .AsNoTracking().Where(relation => relation.GenreId == targetGenre.Id).ToListAsync();

            relations.Should().HaveCount(0);

        }
    }
}
