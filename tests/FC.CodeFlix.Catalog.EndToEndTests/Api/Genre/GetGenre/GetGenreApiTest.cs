﻿using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Domain.Validation;
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

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.GetGenre;


[Collection(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTest: IDisposable
{
    private readonly GetGenreApiTestFixture _fixture;

    public GetGenreApiTest(GetGenreApiTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName =nameof(GetGenre))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task GetGenre()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var targetGenre = exampleGenre[5];

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var (response, output) = await _fixture.ApiClient.Get<ApiResponse<GenreModelOutput>>($"/genres/{targetGenre.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(targetGenre.Name);
        output!.Data.IsActive.Should().Be(targetGenre.IsActive);

    }

    [Fact(DisplayName = nameof(NotFound))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task NotFound()
    {
        var exampleGenre = _fixture.GetExampleListGenres(10);
        var randomGuid = Guid.NewGuid();

        await _fixture.GenrePersistence.InsertList(exampleGenre);

        var (response, output) = await _fixture.ApiClient.Get<ProblemDetails>($"/genres/{randomGuid}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status404NotFound);

        output.Should().NotBeNull();
        output!.Type.Should().Be("NotFound");
        output.Detail.Should().Be($"Genre '{randomGuid}' not found.");
        
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task GetGenreWithRelations()
    {
        var exampleGenres = _fixture.GetExampleListGenres(10);
        
        var targetGenre = exampleGenres[5];

        var random = new Random();

        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        exampleGenres.ForEach(genre =>
        {
            int relationsCount = random.Next(2, exampleCategories.Count -1);

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

        var (response, output) = await _fixture.ApiClient.Get<ApiResponse<GenreModelOutput>>($"/genres/{targetGenre.Id}");

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status200OK);

        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(targetGenre.Name);
        output!.Data.IsActive.Should().Be(targetGenre.IsActive);

        foreach (var category in output.Data.Categories)
        {
            var expectedCategory = exampleCategories.Find(x => x.Id == category.Id);
            category.Name.Should().Be(expectedCategory!.Name);

        }

    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }

}
