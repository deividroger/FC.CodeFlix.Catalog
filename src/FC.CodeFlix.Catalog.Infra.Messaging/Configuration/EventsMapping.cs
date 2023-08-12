using FC.CodeFlix.Catalog.Domain.Events;

namespace FC.CodeFlix.Catalog.Infra.Messaging.Configuration;

internal static class EventsMapping
{
    private static readonly Dictionary<string, string> _routingKeys = new() 
    {
        {typeof(VideoUploadedEvent).Name,"video.created" }
    };

    public static string GetRoutingKey<T>()
        => _routingKeys[typeof(T).Name];

}
