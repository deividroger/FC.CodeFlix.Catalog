namespace FC.CodeFlix.Catalog.Application.Exceptions;

public class NotFoundExcetion : ApplicationException
{
    public NotFoundExcetion(string? message) : base(message)
    {
    }
}
