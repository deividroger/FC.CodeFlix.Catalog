using FluentAssertions;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.SeedWork;

public class AggreateRootTest
{
    [Trait("Domain", "AggreateRoot")]
    [Fact(DisplayName = nameof(RaiseEvent))]
    public void RaiseEvent()
    {
        var domainEvent = new DomainEventFake();
        var aggregate = new AggreateRootFake();

        aggregate.RaiseEvent(domainEvent);

        aggregate.Events.Should().HaveCount(1); 
    }

    [Trait("Domain", "AggreateRoot")]
    [Fact(DisplayName = nameof(ClearEvents))]
    public void ClearEvents()
    {
        var domainEvent = new DomainEventFake();
        var aggregate = new AggreateRootFake();

        aggregate.RaiseEvent(domainEvent);
        
        aggregate.ClearEvents();

        aggregate.Events.Should().HaveCount(0);
    }
}
