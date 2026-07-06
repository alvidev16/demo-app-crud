using Demo.Domain.Exceptions;

namespace Demo.Domain.ValueObjects;

/// <summary>
/// Email address value object: validated on creation and normalized to lower case,
/// so the rest of the system can treat any <see cref="Email"/> as guaranteed-valid.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsValid(value))
            throw new ValidationException(nameof(Email), "A valid email is required.");

        return new Email(value.Trim().ToLowerInvariant());
    }

    private static bool IsValid(string value)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value.Trim();
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
