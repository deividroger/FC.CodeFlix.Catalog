using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;
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
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.CreateCastMember;

[Collection(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTest
{
    private readonly CreateCastMemberTestFixture _fixture;

    public CreateCastMemberTest(CreateCastMemberTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(CreateCastMember))]
    [Trait("Integration/Application","CreateCastMember - Use Cases")]
    public async Task CreateCastMember()
    {
        var actDbContext = _fixture.CreateDbContext();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var repository = new CastMemberRepository(actDbContext);
        var unitOfWork = new UnitOfWork(actDbContext, eventPublisher, logger);

        var useCase = new UseCase.CreateCastMember(repository, unitOfWork);

        var input = new CreateCastMemberInput(_fixture.GetValidName(),_fixture.GetRandomCastMemberType());

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);
        output.Id.Should().NotBeEmpty();
        output.CreatedAt.Should().NotBe(default(DateTime));

        var assertDbContext = _fixture.CreateDbContext(true);

        var castMembers = await  assertDbContext.CastMembers.AsNoTracking().ToListAsync();

        castMembers.Should().HaveCount(1);
        var castMemberFromDb = castMembers[0];

        castMemberFromDb.Name.Should().Be(input.Name);
        castMemberFromDb.Type.Should().Be(input.Type);
        castMemberFromDb.Id.Should().Be(output.Id);
    }
}
