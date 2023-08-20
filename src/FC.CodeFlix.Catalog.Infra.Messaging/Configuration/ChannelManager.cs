using RabbitMQ.Client;

namespace FC.CodeFlix.Catalog.Infra.Storage.Configuration;

public class ChannelManager
{
    private readonly IConnection _connection;
    private IModel? _channel = null;

    private readonly object _lock = new();

    public ChannelManager(IConnection connection)
    {
        _connection = connection;
    }

    public IModel GetChannel()
    {
        lock (_lock)
        {
            if (_channel is null || _channel.IsClosed)
                _channel = _connection.CreateModel();

            return _channel;
        }
    }

}
