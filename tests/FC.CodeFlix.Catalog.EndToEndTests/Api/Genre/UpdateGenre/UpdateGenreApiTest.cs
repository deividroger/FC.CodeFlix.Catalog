using FC.CodeFlix.Catalog.Api.ApiModels.Genre;
using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreApiTestFixture))]
public class UpdateGenreApiTest: IDisposable
{
    private readonly UpdateGenreApiTestFixture _fixture;

    public UpdateGenreApiTest(UpdateGenreApiTestFixture fixture) 
        => _fixture = fixture;

    [Fact(DisplayName =nameof(UpdateGenre))]
    [Trait("EndToEnd/API", "Genre/UpdateGenre - Endpoints")]
    public async Task UpdateGenre()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenre[5];

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var input = new UpdateGenreApiInput(_fixture.GetValidGenreName(), _fixture.GetRandomBoolean());

        var (response, output) = await _fixture.
                                        ApiClient
                                        .Put<ApiResponse<GenreModelOutput>>(
                                                $"/genres/{targetGenre.Id}", input);
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(input.Name);
        output!.Data.IsActive.Should().Be((bool)input.IsActive!);

        var genreFromDb = await _fixture.GenrePersistence.GetById(output.Data.Id);

        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);
    }

    [Fact(DisplayName = nameof(ProblemDetailsWhenNotFound))]
    [Trait("EndToEnd/API", "Genre/UpdateGenre - Endpoints")]
    public async Task ProblemDetailsWhenNotFound()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);

        var randomGuid = Guid.NewGuid();

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var input = new UpdateGenreApiInput(_fixture.GetValidGenreName(), _fixture.GetRandomBoolean());

        var (response, output) = await _fixture.
                                        ApiClient
                                        .Put<ProblemDetails>(
                                                $"/genres/{randomGuid}", input);
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);

        output.Should().NotBeNull();
        output!.Detail.Should().Be($"Genre '{randomGuid}' not found.");
        output.Title.Should().Be("Not Found");
        output.Type.Should().Be($"NotFound");
        output.Status.Should().Be((int)StatusCodes.Status404NotFound);

    }


    [Fact(DisplayName = nameof(UpdateGenreWithRelations))]
    [Trait("EndToEnd/API", "Genre/UpdateGenre - Endpoints")]
    public async Task UpdateGenreWithRelations()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);

        var targetGenre = exampleGenres[5];

        var random = new Random();

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(2, exampleCategories.Count - 1);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        int newRelationsCount = random.Next(2, exampleCategories.Count - 1);
        var newRelatedCategoriesIds = new List<Guid>();

        for (int i = 0; i < newRelationsCount; i++)
        {
            int selectedCategoryIndex = exampleCategories.Count - 1;
            var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

            if (!newRelatedCategoriesIds.Contains(selected.Id))
                newRelatedCategoriesIds.Add(selected.Id);
        }


        await _fixture.GenrePersistence.InsertList(exampleGenres);
        await _fixture.CategoryPersistence.InsertList(exampleCategories);
        await _fixture.GenrePersistence.InsertGenresCategoriesRelationsList(genresCategories);



        var input = new UpdateGenreApiInput(_fixture.GetValidGenreName(),
                                           _fixture.GetRandomBoolean(), newRelatedCategoriesIds);

        var (response, output) = await _fixture.
                                        ApiClient
                                        .Put<ApiResponse<GenreModelOutput>>(
                                                $"/genres/{targetGenre.Id}", input);
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(input.Name);
        output!.Data.IsActive.Should().Be((bool)input.IsActive!);

        var genreFromDb = await _fixture.GenrePersistence.GetById(output.Data.Id);

        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

        var relatedCategoriesIdsFromOutput = output.Data.Categories.Select(relation => relation.Id).ToList();

        relatedCategoriesIdsFromOutput.Should().BeEquivalentTo(newRelatedCategoriesIds);

        var genresCategoriesFromDb = await _fixture.GenrePersistence.GetGenresCategoriesRelationsById(targetGenre.Id);

        var relatedCategoriesIdsFromDb = genresCategoriesFromDb.Select(x =>x.CategoryId).ToList();

        relatedCategoriesIdsFromDb.Should().BeEquivalentTo(newRelatedCategoriesIds);

    }

    [Fact(DisplayName = nameof(ErrorWhenInvalidRelation))]
    [Trait("EndToEnd/API", "Genre/UpdateGenre - Endpoints")]
    public async Task ErrorWhenInvalidRelation()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenre[5];

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var randomGuid = Guid.NewGuid();

        var input = new UpdateGenreApiInput(_fixture.GetValidGenreName(),   
                                            _fixture.GetRandomBoolean()
                                            ,new List<Guid>() { randomGuid });

        var (response, output) = await _fixture.
                                        ApiClient
                                        .Put<ProblemDetails>(
                                                $"/genres/{targetGenre.Id}", input);
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status422UnprocessableEntity);

        output.Should().NotBeNull();
        output!.Type.Should().Be("RelatedAggregate");
        output.Detail.Should().Be($"Related category Id (or ids) not found : {randomGuid}");
    }


    [Fact(DisplayName = nameof(PersistsRelationsWhenNotPresentInInput))]
    [Trait("EndToEnd/API", "Genre/UpdateGenre - Endpoints")]
    public async Task PersistsRelationsWhenNotPresentInInput()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);

        var targetGenre = exampleGenres[5];

        var random = new Random();

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(2, exampleCategories.Count - 1);

            for (int i = 0; i < relationsCount; i++)
            {
                int selectedCategoryIndex = exampleCategories.Count - 1;
                var selected = exampleCategories[random.Next(0, selectedCategoryIndex)];

                if (!genre.Categories.Contains(selected.Id))
                    genre.AddCategory(selected.Id);
            }

        });

        var genresCategories = new List<GenresCategories>();

        exampleGenres
                .ForEach(genre => genre
                        .Categories
                        .ToList()
                .ForEach(categoryId =>
                        genresCategories
                        .Add(new GenresCategories(categoryId, genre.Id)
           )));

        
        await _fixture.GenrePersistence.InsertList(exampleGenres);
        await _fixture.CategoryPersistence.InsertList(exampleCategories);
        await _fixture.GenrePersistence.InsertGenresCategoriesRelationsList(genresCategories);



        var input = new UpdateGenreApiInput(_fixture.GetValidGenreName(),
                                           _fixture.GetRandomBoolean());

        var (response, output) = await _fixture.
                                        ApiClient
                                        .Put<ApiResponse<GenreModelOutput>>(
                                                $"/genres/{targetGenre.Id}", input);
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(input.Name);
        output!.Data.IsActive.Should().Be((bool)input.IsActive!);

        var genreFromDb = await _fixture.GenrePersistence.GetById(output.Data.Id);

        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);

        var relatedCategoriesIdsFromOutput = output.Data.Categories.Select(relation => relation.Id).ToList();

        relatedCategoriesIdsFromOutput.Should().BeEquivalentTo(targetGenre.Categories);

        var genresCategoriesFromDb = await _fixture.GenrePersistence.GetGenresCategoriesRelationsById(targetGenre.Id);

        var relatedCategoriesIdsFromDb = genresCategoriesFromDb.Select(x => x.CategoryId).ToList();

        relatedCategoriesIdsFromDb.Should().BeEquivalentTo(targetGenre.Categories);

    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }


}
