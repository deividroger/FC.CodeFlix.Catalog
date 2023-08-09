namespace FC.CodeFlix.Catalog.Application.Interfaces;

public interface IStorageService
{
    Task <string> Upload(string fileName,Stream fileStream, string contentType, CancellationToken cancellationToken);

    Task Delete(string filePath, CancellationToken cancellationToken);
}
