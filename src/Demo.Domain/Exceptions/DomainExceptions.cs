namespace Demo.Domain.Exceptions;

/// <summary>Base class for all business-rule (domain) errors.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>Thrown when one or more validation rules fail. Maps to HTTP 400.</summary>
public sealed class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string> { [field] = message }) { }
}

/// <summary>Thrown when a requested entity does not exist. Maps to HTTP 404.</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.") { }
}

/// <summary>Thrown when an operation conflicts with existing data (e.g. duplicate SKU). Maps to HTTP 409.</summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Thrown when credentials are invalid. Maps to HTTP 401.</summary>
public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("Invalid email or password.") { }
}
