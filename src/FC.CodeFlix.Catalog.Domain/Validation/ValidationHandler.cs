
namespace FC.CodeFlix.Catalog.Domain.Validation;

public abstract class  ValidationHandler
{
    public void HandleError(string message)
        => HandleError(new ValidationError(message));

    public abstract void HandleError(ValidationError error);

}
