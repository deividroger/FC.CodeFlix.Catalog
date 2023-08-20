﻿using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Genre.CreateGenre;

[Collection(nameof(CreateGenreApiTestFixture))]
public class CreateGenreApiTest: IDisposable
{
    private readonly CreateGenreApiTestFixture _fixture;

    public CreateGenreApiTest(CreateGenreApiTestFixture fixture) 
        => _fixture = fixture;

    [Fact(DisplayName =nameof(CreateGenre))]
    [Trait("EndToEnd/API", "Genre/Create - Endpoints")]
    public async Task CreateGenre()
    {
        var apiInput = new CreateGenreInput(_fixture.GetValidCategoryName(),_fixture.GetRandomBoolean());

        var (response, output) = await _fixture.ApiClient
                                               .Post<ApiResponse<GenreModelOutput>>($"/genres", apiInput);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Data.Id.Should().NotBeEmpty();
        
        output.Data.Name.Should().Be(apiInput.Name);
        output.Data.IsActive.Should().Be(apiInput.IsActive);

        output.Data.Categories.Should().HaveCount(0);

        var genreFromDb = await _fixture.GenrePersistence.GetById(output.Data.Id);

        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(apiInput.Name);
        genreFromDb.IsActive.Should().Be(apiInput.IsActive);

    }


    [Fact(DisplayName = nameof(CreateGenreWithRelations))]
    [Trait("EndToEnd/API", "Genre/Create - Endpoints")]
    public async Task CreateGenreWithRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        await _fixture.CategoryPersistence.InsertList(exampleCategories);

        var relatedCategories = exampleCategories.Skip(3).Take(3).Select(x=> x.Id).ToList();


        var apiInput = new CreateGenreInput(_fixture.GetValidCategoryName(), _fixture.GetRandomBoolean(), relatedCategories);

        var (response, output) = await _fixture.ApiClient
                                               .Post<ApiResponse<GenreModelOutput>>($"/genres", apiInput);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status201Created);

        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Data.Id.Should().NotBeEmpty();

        output.Data.Name.Should().Be(apiInput.Name);
        output.Data.IsActive.Should().Be(apiInput.IsActive);

        output.Data.Categories.Should().HaveCount(relatedCategories.Count);
        var outputRelatedCategoriesIds = output.Data.Categories.Select(x => x.Id).ToList();

        outputRelatedCategoriesIds.Should().BeEquivalentTo(relatedCategories);

        var genreFromDb = await _fixture.GenrePersistence.GetById(output.Data.Id);

        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(apiInput.Name);
        genreFromDb.IsActive.Should().Be(apiInput.IsActive);

        var relationsFromDb = await _fixture.GenrePersistence.GetGenresCategoriesRelationsById(output.Data.Id);

        relationsFromDb.Should().NotBeNull();
        relationsFromDb.Should().HaveCount(relatedCategories.Count);

        var relatedCategoriesIdsFromDb = relationsFromDb.Select(x => x.CategoryId).ToList();

        relatedCategoriesIdsFromDb.Should().BeEquivalentTo(relatedCategories);

    }

   
    [Fact(DisplayName = nameof(ErrorWithInvalidRelations))]
    [Trait("EndToEnd/API", "Genre/Create - Endpoints")]
    public async Task ErrorWithInvalidRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);

        var invalidCategoryId = Guid.NewGuid();

        await _fixture.CategoryPersistence.InsertList(exampleCategories);

        var relatedCategories = exampleCategories.Skip(3).Take(3).Select(x => x.Id).ToList();

        relatedCategories.Add(invalidCategoryId);

        var apiInput = new CreateGenreInput(_fixture.GetValidCategoryName(), _fixture.GetRandomBoolean(), relatedCategories);

        var (response, output) = await _fixture.ApiClient
                                               .Post<ProblemDetails>($"/genres", apiInput);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status422UnprocessableEntity);

        output.Should().NotBeNull();
        output!.Type.Should().Be("RelatedAggregate");
        output.Detail.Should().Be($"Related category Id (or ids) not found : {invalidCategoryId}");
  
    }

    public void Dispose()
    {
        _fixture.CleanPersistence();
    }

}