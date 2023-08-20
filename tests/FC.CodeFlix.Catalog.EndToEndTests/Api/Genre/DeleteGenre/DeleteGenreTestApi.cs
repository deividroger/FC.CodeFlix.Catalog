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

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreTestApiFixture))]
public class DeleteGenreTestApi: IDisposable
{
    private readonly DeleteGenreTestApiFixture _fixture;

    public DeleteGenreTestApi(DeleteGenreTestApiFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName =nameof(DeleteGenreAsync))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task DeleteGenreAsync()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenre[5];

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var (response, output) = await _fixture.ApiClient.Delete<object>($"/genres/{targetGenre.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status204NoContent);

        output.Should().BeNull();

        var genreDb = await _fixture.GenrePersistence.GetById(targetGenre.Id);

        genreDb.Should().BeNull();
        
    }

    [Fact(DisplayName = nameof(WhenNotFound404))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task WhenNotFound404()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenre[5];

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var randomGuid = Guid.NewGuid();


        var (response, output) = await _fixture.ApiClient.Delete<ProblemDetails>($"/genres/{randomGuid}");

        response.Should().NotBeNull();
        output.Should().NotBeNull();

        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);

        output!.Type.Should().Be("NotFound");
        output.Detail.Should().Be($"Genre '{randomGuid}' not found.");



    }


    [Fact(DisplayName = nameof(DeleteGenreWithRelationsAsync))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task DeleteGenreWithRelationsAsync()
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

        var (response, output) = await _fixture.ApiClient.Delete<object>($"/genres/{targetGenre.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status204NoContent);

        output.Should().BeNull();

        var genreDb = await _fixture.GenrePersistence.GetById(targetGenre.Id);

        genreDb.Should().BeNull();

        var relations = await _fixture.GenrePersistence.GetGenresCategoriesRelationsById(targetGenre.Id);

        relations.Should().HaveCount(0);

    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }

}
