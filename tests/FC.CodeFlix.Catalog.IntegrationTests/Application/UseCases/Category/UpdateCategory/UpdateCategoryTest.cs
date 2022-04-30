using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;
using useCase = FC.CodeFlix.Catalog.Application.UseCases.Category.UpdateCategory;
namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest
{
    private readonly UpdateCategoryTestFixture _fixture;

    public UpdateCategoryTest(UpdateCategoryTestFixture fixture)
        => _fixture = fixture;


    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Integration/Application", "Update category - Use Cases")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
     parameters: 5,
     MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategory(DomainEntity.Category exampleCategory,
                                 UpdateCategoryInput input)
    {
        var dbContext = _fixture.CreateDbContext();

        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        var trakingInfo = await dbContext.AddAsync(exampleCategory);
        await dbContext.SaveChangesAsync();

        trakingInfo.State = EntityState.Detached;

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);


        var useCase = new useCase.UpdateCategory(repository, unitOfWork);

        var output = await useCase.Handle(input, CancellationToken.None);


        var dbCategory = await (_fixture.CreateDbContext(true))
                                        .Categories.FindAsync(output.Id);


        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.IsActive.Should().Be((bool)input.IsActive!);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);


        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be((bool)input.IsActive!);


    }


    [Theory(DisplayName = nameof(UpdateCategoryWithoutIsActive))]
    [Trait("Integration/Application", "Update category - Use Cases")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
    parameters: 5,
  MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategoryWithoutIsActive(DomainEntity.Category exampleCategory,
                              UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(exampleInput.Id, exampleCategory.Name, exampleCategory.Description);

        var dbContext = _fixture.CreateDbContext();

        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        var trakingInfo = await dbContext.AddAsync(exampleCategory);
        await dbContext.SaveChangesAsync();

        trakingInfo.State = EntityState.Detached;

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);


        var useCase = new useCase.UpdateCategory(repository, unitOfWork);

        var output = await useCase.Handle(input, CancellationToken.None);


        var dbCategory = await (_fixture.CreateDbContext(true))
                                        .Categories.FindAsync(output.Id);


        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);


        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);


    }


    [Theory(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("Integration/Application", "Update category - Use Cases")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
    parameters: 5,
    MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategoryOnlyName(DomainEntity.Category exampleCategory,
                          UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(exampleInput.Id, exampleCategory.Name);

        var dbContext = _fixture.CreateDbContext();

        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        var trakingInfo = await dbContext.AddAsync(exampleCategory);
        await dbContext.SaveChangesAsync();

        trakingInfo.State = EntityState.Detached;

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);


        var useCase = new useCase.UpdateCategory(repository, unitOfWork);

        var output = await useCase.Handle(input, CancellationToken.None);


        var dbCategory = await (_fixture.CreateDbContext(true))
                                        .Categories.FindAsync(output.Id);


        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);


        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(exampleCategory.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);


    }

    [Fact(DisplayName = nameof(UpdateThrowsWhenNotFoundCategory))]
    [Trait("Integration/Application", "Update category - Use Cases")]
    public async Task UpdateThrowsWhenNotFoundCategory()
    {
        var input = _fixture.GetValidInput();

        var dbContext = _fixture.CreateDbContext();

        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());

        await dbContext.SaveChangesAsync();

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);


        var useCase = new useCase.UpdateCategory(repository, unitOfWork);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>().WithMessage($"category '{input.Id}' not found.");

    }


    [Theory(DisplayName = nameof(UpdateThrowsWhenCantInstantiateCategory))]
    [Trait("Integration/Application", "Update category - Use Cases")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs),
    parameters: 6,
    MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateThrowsWhenCantInstantiateCategory(
                      UpdateCategoryInput input, string expectedExceptionMessage)
    {


        var dbContext = _fixture.CreateDbContext();

        var exampleCategories = _fixture.GetExampleCategoriesList();

        await dbContext.AddRangeAsync(exampleCategories);
        await dbContext.SaveChangesAsync();


        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);


        var useCase = new useCase.UpdateCategory(repository, unitOfWork);

        input.Id = exampleCategories[0].Id;

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<EntityValidationException>().WithMessage(expectedExceptionMessage);

    }
}
