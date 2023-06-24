namespace FC.CodeFlix.Catalog.Domain.Validation;

public abstract class Validator
{
    protected readonly ValidationHandler _handler;
    public Validator(ValidationHandler handler)
        => _handler = handler;
    
    public abstract void Validate();
}
