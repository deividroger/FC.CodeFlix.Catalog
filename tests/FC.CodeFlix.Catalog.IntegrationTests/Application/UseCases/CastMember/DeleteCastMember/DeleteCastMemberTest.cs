
using FC.CodeFlix.Catalog.Application;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;
using FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.CastMember.DeleteCastMember;

namespace FC.CodeFlix.Catalog.IntegrationTests.Application.UseCases.CastMember.DeleteCastMember;

[Collection(nameof(CastMemberUseCasesBaseFixture))]
public class DeleteCastMemberTest
{
    private readonly CastMemberUseCasesBaseFixture _fixture;

    public DeleteCastMemberTest(CastMemberUseCasesBaseFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Application", "DeleteCastMember - Use Cases")]
    public async Task Delete()
    {
        var example = _fixture.GetExampleCastMember();

        var arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddAsync(example);
        await arrangeDbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var repository = new CastMemberRepository(actDbContext);
        var unitOfWork = new UnitOfWork(actDbContext,eventPublisher,logger);

        var useCase = new UseCase.DeleteCastMember(repository, unitOfWork);
        var input = new UseCase.DeleteCastMemberInput(example.Id);
        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);


        var output = assertDbContext.CastMembers.Where(x => x.Id == example.Id).SingleOrDefault();

        output.Should().BeNull();

    }

    [Fact(DisplayName = nameof(ThrownWhenNotFound))]
    [Trait("Integration/Application", "DeleteCastMember - Use Cases")]
    public async Task ThrownWhenNotFound()
    {
        var actDbContext = _fixture.CreateDbContext(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var eventPublisher = new DomainEventPublisher(serviceProvider);
        var logger = serviceProvider.GetRequiredService<ILogger<UnitOfWork>>();

        var repository = new CastMemberRepository(actDbContext);
        var unitOfWork = new UnitOfWork(actDbContext, eventPublisher, logger);
        var useCase = new UseCase.DeleteCastMember(repository, unitOfWork);
        var input = new UseCase.DeleteCastMemberInput(Guid.NewGuid());

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Cast Member '{input.Id} not found.'");
    }
}