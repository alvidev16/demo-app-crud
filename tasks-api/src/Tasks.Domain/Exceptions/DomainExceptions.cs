namespace Tasks.Domain.Exceptions;

/// <summary>Base class for business-rule errors.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>One or more validation rules failed. Maps to HTTP 400.</summary>
public sealed class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string> errors)
        : base("One or more validation errors occurred.") => Errors = errors;

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string> { [field] = message }) { }
}

/// <summary>Requested entity does not exist (or is not visible to the caller). Maps to HTTP 404.</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.") { }
}
