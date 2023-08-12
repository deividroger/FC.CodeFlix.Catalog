using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Events;
using FC.CodeFlix.Catalog.Domain.SeedWork;

namespace FC.CodeFlix.Catalog.Application.EventHandlers;

public class SendToEncoderEventHandler : IDomainEventHandler<VideoUploadedEvent>
{
    private readonly IMessageProducer _messageProducer;

    public SendToEncoderEventHandler(IMessageProducer messageProducer)
        => _messageProducer = messageProducer;

    public async Task HandleAsync(VideoUploadedEvent domainEvent, CancellationToken cancellationToken)
       => await _messageProducer.SendMessageAsync(domainEvent, cancellationToken);
}
