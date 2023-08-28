using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Infra.Messaging.Configuration;
using FC.CodeFlix.Catalog.Infra.Messaging.Dtos;
using FC.CodeFlix.Catalog.Infra.Messaging.JsonPolicies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FC.CodeFlix.Catalog.Infra.Messaging.Consumer;

public class VideoEncodedEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoEncodedEventConsumer> _logger;
    private readonly string _queue;
    private readonly IModel _channel;

    public VideoEncodedEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<VideoEncodedEventConsumer> logger,
        IOptions<RabbitMQConfiguration> configuration,
        IModel channel)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queue = configuration.Value.VideoEncodedQueue!;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10_000, stoppingToken);
        }
        _channel.Dispose();
        _logger.LogWarning("Disposing channel.");
    }



    private void OnMessageReceived(object? sender, BasicDeliverEventArgs eventArgs)
    {
        var messagesString = string.Empty;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            messagesString = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            _logger.LogDebug(messagesString);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new JsonSnakeCasePolicy()
            };

            var message = JsonSerializer.Deserialize<VideoEncodedMessageDTO>(messagesString, jsonOptions);

            var input = GetUpdateMediaStatusInput(message!);

            mediator.Send(input, CancellationToken.None).Wait();

            _channel.BasicAck(eventArgs.DeliveryTag, false);

        }
        catch (Exception ex) when (ex is EntityValidationException or NotFoundException)
        {

            _logger.LogError(ex,
                "There was a business error in the message processing: {deliveryTag}, {message}",
                eventArgs.DeliveryTag, messagesString);

            _channel.BasicNack(eventArgs.DeliveryTag, false, false);
        }catch(Exception ex)
        {
            _logger.LogError(ex,
                "There was a unexpected error in the message processing: {deliveryTag}, {message}",
                eventArgs.DeliveryTag, messagesString);

            _channel.BasicNack(eventArgs.DeliveryTag, false, true);
        }
    }

    private UpdateMediaStatusInput GetUpdateMediaStatusInput(VideoEncodedMessageDTO message)
    {

        if (message.Video != null)
        {
            return new UpdateMediaStatusInput(Guid.Parse(message.Video.ResourceId),
                MediaStatus.Completed,
                EncodedPath: message.Video.FullEncodedVideoFilePath);
        }

        return new UpdateMediaStatusInput(Guid.Parse(message.Message!.ResourceId!),
                MediaStatus.Error,
                ErrorMessage: message.Error);
    }
}
