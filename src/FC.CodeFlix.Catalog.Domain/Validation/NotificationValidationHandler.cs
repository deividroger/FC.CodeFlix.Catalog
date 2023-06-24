
namespace FC.CodeFlix.Catalog.Domain.Validation;

public class NotificationValidationHandler : ValidationHandler
{
    private readonly List<ValidationError> _errors;

    public NotificationValidationHandler()
        => _errors = new();

    public bool HasErrors() 
        => _errors.Any();

    public IReadOnlyCollection<ValidationError> Errors 
        => _errors;

    public override void HandleError(ValidationError error)
        => _errors.Add(error);
}
