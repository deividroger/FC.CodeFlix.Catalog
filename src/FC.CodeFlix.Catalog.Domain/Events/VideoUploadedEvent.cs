using FC.CodeFlix.Catalog.Domain.SeedWork;

namespace FC.CodeFlix.Catalog.Domain.Events;

public class VideoUploadedEvent: DomainEvent
{
    public VideoUploadedEvent(Guid resourceId, string filePath)
    {
        ResourceId = resourceId;
        FilePath = filePath;
    }

    public Guid ResourceId { get; set; }

    public string FilePath { get; set; }
}
