using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.ListVideos;

[CollectionDefinition(nameof(ListVideosTestFixture))]
public class ListVideosTestFixtureCollection : ICollectionFixture<ListVideosTestFixture>
{
}

public class ListVideosTestFixture : VideoBaseFixture
{
    public List<DomainEntity.Video> CreateExamplesVideosList()
        => Enumerable
            .Range(1, Random.Shared.Next(2, 10))
            .Select(_ => GetValidVideoWithAllProperties()).ToList();

    public (List<DomainEntity.Video> Videos,
            List<DomainEntity.Category> Categories,
        List<DomainEntity.Genre> Genres,
        List<DomainEntity.CastMember> CastMembers
            ) CreateExamplesVideosListWithRelations()
    {
        var itemsQuantityToBeCreated = Random.Shared.Next(2, 10);
        var videos = Enumerable
                        .Range(1, Random.Shared.Next(2, itemsQuantityToBeCreated))
                        .Select(_ => GetValidVideoWithAllProperties()).ToList();

        List<DomainEntity.Category> categories = new();
        List<DomainEntity.Genre> genres = new();
        List<DomainEntity.CastMember> castMembers = new();

        videos.ForEach(video =>
        {
            video.RemoveAllCategories();
            var qtdCategories = Random.Shared.Next(2, 5);
            for (var i = 0; i < qtdCategories; i++)
            {
                var category = GetExampleCategory();
                categories.Add(category);
                video.AddCategory(category.Id);
            }

            video.RemoveAllGenre();
            var qtdGenres = Random.Shared.Next(2, 5);
            for (var i = 0; i < qtdGenres; i++)
            {
                var genre = GetExampleGenre();
                genres.Add(genre);
                video.AddGenre(genre.Id);
            }

            video.RemoveAllCastMembers();
            var qtdCastMembers = Random.Shared.Next(2, 5);
            for (var i = 0; i < qtdCastMembers; i++)
            {
                var castMember = GetExampleCastMember();
                castMembers.Add(castMember);
                video.AddCastMember(castMember.Id);
            }
        });

        return (videos, categories, genres, castMembers);
    }

    public List<DomainEntity.Video> CreateExampleVideoListWithoutRelations()
    {
        var itemsQuantityToBeCreated = Random.Shared.Next(2, 10);
        return Enumerable
                        .Range(1, Random.Shared.Next(2, itemsQuantityToBeCreated))
                        .Select(_ => GetValidVideo())
                        .ToList();
    }

    private DomainEntity.CastMember GetExampleCastMember()
        => new(GetValidName(), GetRandomCastMemberType());

    private string GetValidName()
     => Faker.Name.FullName();

    private CastMemberType GetRandomCastMemberType()
        => (CastMemberType)(new Random()).Next(1, 2);

    private DomainEntity.Genre GetExampleGenre()
        => new(GetValidGenreName(), GetRandomBoolean());

    private string GetValidGenreName()
        => Faker.Commerce.Categories(1)[0];
    private string GetValidCategoryName()
    {
        var categoryName = "";

        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];

        if (categoryName.Length > 255)
            categoryName = categoryName[..255];

        return categoryName;
    }

    private DomainEntity.Category GetExampleCategory()
        => new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

    private string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();


        if (categoryDescription.Length > 10_000)
            categoryDescription = categoryDescription[..10_000];

        return categoryDescription;
    }


}
